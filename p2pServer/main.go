package main

import (
    "fmt"
    "net"
    "os"
	"time"
	"encoding/json"
	"strings"
)

///TCP Listener
var listener net.Listener = nil

var Id = 1000

///Server connection info
const (
    CONN_HOST = "localhost"
    CONN_PORT = "3333"
	CONN_TYPE = "tcp"
)

///Connected client
type connection struct {
	Conn net.Conn
	IsConnected bool
	Lobby *lobby // index of the registered lobby.
	Id int
}

///Lobby
type lobby struct {
	Connections [8]*connection // A lobby can hold maximum 8 players.
	Host int // Host index
}

///Disconnected: Disconnected player id, Connected: Connected player id.
func LobbyUpdate (Lobby *lobby, Disconnected int, Connected int){
	var outgoing = OutgoingMessage { Purpose : 2 }
	outgoing.LobbyUpdate = _OnLobbyUpdate { DC : Disconnected, C : Connected }

	var NeedHost = false
	if Lobby.Host == -1 || !Lobby.Connections[Lobby.Host].IsConnected {
		// Need new host.
		NeedHost = true
	}

	for e:=range Lobby.Connections {
		if Lobby.Connections[e] != nil {
			if Lobby.Connections[e].IsConnected{
				if NeedHost{
					Lobby.Host = e // New host assigned.
					NeedHost = false
				}
				
				outgoing.LobbyUpdate.IsHost = e == Lobby.Host
				send (Lobby.Connections[e], &outgoing)
			}
		}
	}
}

/// ALL CONNECTIONS
var Connections [64]connection // Maximum 64 players
var Lobbies [8]lobby // In 8 lobbies

///Returns true if any connected player in this lobby. It means lobby is held.
func AnyPlayerInLobby (Lobby *lobby) (bool){
	fmt.Println ("Any player in lobby ()")
	for i := range Lobby.Connections {
		if Lobby.Connections[i].IsConnected {
			fmt.Println ("Someone in lobby")
			return true
		}
	}

	fmt.Println ("Lobby is empty")
	return false
}

///Returns true if any connected player in this lobby. It means lobby is held.
func RemoveConnectionFromLobby (Connection *connection) {
	if Connection.Lobby == nil {
		return
	}

	for i := range Connection.Lobby.Connections {
		if Connection.Lobby.Connections[i] == Connection {
			var dId = Connection.Id
			var dCon connection
			dCon.IsConnected = false
			Connection.Lobby.Connections[i] = &dCon
			fmt.Println ("Connection removed from lobby")
			
			LobbyUpdate (Connection.Lobby, dId, 0)
		}
	}
}

///Returns true if joinable. and returns the index
func GetLobbySlot (Lobby *lobby) (bool, int){
	for i := range Lobby.Connections {
		if !Lobby.Connections[i].IsConnected {
			return true, i
		}
	}

	return false, -1
}

func addconnection (newconnection net.Conn) {
	for i := range Connections {
		if !Connections[i].IsConnected {
			Connections[i].Conn = newconnection
			Connections[i].IsConnected = true
			Connections[i].Id = Id
			Id = Id + 1
			fmt.Println ("Client connected", Connections[i].Id)
			return
		}
	}
	
	fmt.Println("Server is full!")
}

//Define connections in lobbies
func DefineConnectionsInLobbies () {
	for i := range Lobbies {
		for e:=range Lobbies[i].Connections {
			var dCon connection
			Lobbies[i].Connections[e] = &dCon
		}
	}
}

func main() {
    // Listen for incoming connections.
    r, err := net.Listen(CONN_TYPE, CONN_HOST+":"+CONN_PORT)
    if err != nil {
        fmt.Println("Error listening:", err.Error())
        os.Exit(1)
    }
    // Close the listener when the application closes.
	defer r.Close()
	
	listener = r

	DefineConnectionsInLobbies()
	
	fmt.Println("Listening on " + CONN_HOST + ":" + CONN_PORT)

	go StartPing ()

	go StartListen()

	WaitForConnection ()
}

func StartPing (){

	for i := range Connections { // Start pinging
		go PingToConnection (&Connections[i])
	}
}

func StartListen (){
	for i := range Connections { // Listen messages
		go ListenTheConnection (&Connections [i])
	}
}

/// Listen for the new connections
func WaitForConnection () {
	second := time.Tick(time.Second)

	for {
        select {
		case <-second:
			conn, err := listener.Accept()

			if err != nil {
				fmt.Println("Error accepting: ", err.Error())
				//os.Exit(1)
			} else {
			// New connection here.
				addconnection (conn)
			}
		}
	}
}

func PingToConnection (conn *connection){
	second := time.Tick(time.Second)

	defer func() {
		fmt.Println("Closing connection...")
		conn.Conn.Close()
	}()

	for {
        select {
		case <-second:
			// Ping the connection.
			if conn.IsConnected {
				var msg OutgoingMessage = OutgoingMessage{ Purpose : -1 }
				var success bool = send (conn, &msg)

				if !success {
					conn.IsConnected = false
					RemoveConnectionFromLobby (conn)
					conn.Lobby = nil;
					fmt.Println("Client disconnected")
				}
			}
		}
	}
}

func ListenTheConnection (conn *connection){
	millisecond := time.Tick(time.Millisecond)

	defer func() {
		fmt.Println("Closing connection...")
		conn.Conn.Close()
	}()

	for {
        select {
		case <- millisecond:
			if conn.IsConnected {
				//Read the incoming connection into the buffer.
				buf := make([]byte, 65536)
				//Read the incoming connection into the buffer.
				length, error := conn.Conn.Read(buf)

				if error == nil {
					var incoming string = string(buf[:length])
					ProcessData (incoming, conn)
				}/* else {
					fmt.Println (conn.err)
				}*/
			}
		}
	}
}

func send (conn *connection, msg *OutgoingMessage) (bool) {
	e, err := json.Marshal(msg)
    if err != nil {
        return false
    }
		// Send a response back to person contacting us.
	_, error := conn.Conn.Write([]byte(e))

	if error != nil {
		return false
	}

	return true
}

///Process the incoming message, and relay the message to lobby if the connection is in a lobby.
func ProcessData (data string, sender *connection) {
	var netMessage IncomingMessage
	if err := json.NewDecoder(strings.NewReader(data)).Decode(&netMessage); err != nil {
			/// Message cannot be parsed.
		return
	}

	var outgoing OutgoingMessage = OutgoingMessage{}
	outgoing.Purpose = netMessage.Purpose // Add the purpose

	fmt.Println ("Incoming message: ", netMessage.Purpose, sender.Id)

	switch netMessage.Purpose {
		case 0: // Join Lobby
			outgoing.JoinLobby = _OnJoinLobby{ Success: false }
			fmt.Println ("Join lobby request")

			if sender.Lobby != nil{
				fmt.Println ("Join lobby request: but already in a lobby. Leave it first.")
				send (sender, &outgoing)
				return;
			}

			for i := range Lobbies {
				if AnyPlayerInLobby (&Lobbies[i]){
					joinable, index := GetLobbySlot (&Lobbies[i])
					if !joinable {
						continue
					}else{
						// Join this lobby.			
						Lobbies[i].Connections[index] = sender
						sender.Lobby = &Lobbies[i]
						fmt.Println ("Joined.")
						outgoing.JoinLobby.Success = true
						LobbyUpdate (&Lobbies[i], 0, sender.Id)
						break
					}
				}else{
					// Create a lobby.
					Lobbies[i].Connections[0] = sender
					sender.Lobby = &Lobbies[i]
					fmt.Println ("Created.")
					outgoing.JoinLobby.Success = true
					LobbyUpdate (&Lobbies[i], 0, sender.Id)
					break
				}
			}

			outgoing.JoinLobby.Id = sender.Id
			send (sender, &outgoing) // Send for callback.
		case 1: // Relay to Lobby
		
		if sender.Lobby == nil{
			return; // no lobby. no relay.
		}

		outgoing.RelayToLobby = _OnP2P { Msg : netMessage.RelayToLobby.Msg, Sender: sender.Id }

		for e:=range sender.Lobby.Connections {
			if sender.Lobby.Connections[e] != nil && sender.Lobby.Connections[e].IsConnected {
				send (sender.Lobby.Connections[e], &outgoing)
			}
		}
	}
}