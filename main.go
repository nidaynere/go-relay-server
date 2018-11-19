///TODO need parser to split to handle sent string

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
}

///Lobby
type lobby struct {
	Connections [8]*connection // A lobby can hold maximum 8 players.
}

/// ALL CONNECTIONS
var Connections [8000]connection // Maximum 8000 players
var Lobbies [1000]lobby // In 1000 lobbies

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
			var dCon connection
			Connection.Lobby.Connections[i] = &dCon
			fmt.Println ("Connection removed from lobby")
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

func CloseLobby (Lobby *lobby){
	for i := range Lobby.Connections {
		Lobby.Connections[i].IsConnected = false
	}
}

func addconnection (newconnection net.Conn) {
	for i := range Connections {
		if !Connections[i].IsConnected {
			Connections[i].Conn = newconnection
			Connections[i].IsConnected = true
			fmt.Println ("Client connected")
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

	second := time.Tick(time.Second)
	millisecond := time.Tick(time.Millisecond)
    for {
        select {
        case <-second:
			go WaitForConnection()
			go handleRequest()
		case <-millisecond:
			go handleMessages()
		}
	}
}

/// Listen for the new connections
func WaitForConnection () {
	conn, err := listener.Accept()

	if err != nil {
		fmt.Println("Error accepting: ", err.Error())
		//os.Exit(1)
	} else {
	// New connection here.
		addconnection (conn)
	}
}

// Handles the connections. Pings them every one second
func handleRequest() {
	for i := range Connections {
		if Connections[i].IsConnected {
			// Ping the connection.

			var success bool = sendMessage (&Connections[i], OutgoingMessage{ Purpose : -1 })
			
			if !success {
				Connections [i].IsConnected = false
				RemoveConnectionFromLobby (&Connections [i])
				Connections[i].Lobby = nil;
				fmt.Println("Client disconnected")
				continue
			}
		}
	}
}

func handleMessages () {
	for i := range Connections {
		if Connections[i].IsConnected {
				//Make a buffer to hold incoming data.
				buf := make([]byte, 1024)
				//Read the incoming connection into the buffer.
				length, error := Connections[i].Conn.Read(buf)

				if error == nil {
					var incoming string = string(buf[:length])
					//fmt.Println("sender's lobby: ", connections[i].lobby)
					//go sendMessage (&connections[i], "mesajini aldim kankey")
					// TODO: Relay the message to all lobby members
					ProcessData (incoming, &Connections[i])
				}
		}
	}			
}

//usage: sendMessage (&connections[i], incoming) // send back the incoming message
func sendMessage (conn *connection, msg OutgoingMessage) (bool) {
	if conn.Conn == nil{
		return false
	}

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

	fmt.Println ("incoming message purpose: ", netMessage.Purpose)

	var outgoing OutgoingMessage = OutgoingMessage{}
	outgoing.Purpose = netMessage.Purpose // Add the purpose

	switch netMessage.Purpose {
		case 0: // Join Lobby
			outgoing.JoinLobby = _OnJoinLobby{ Success: false }
			fmt.Println ("Join lobby request")

			if sender.Lobby != nil{
				fmt.Println ("Join lobby request: but already in a lobby. Leave it first.")
				sendMessage (sender, outgoing)
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
						break
					}
				}else{
					// Create a lobby.
					Lobbies[i].Connections[0] = sender
					sender.Lobby = &Lobbies[i]
					fmt.Println ("Created.")
					outgoing.JoinLobby.Success = true
					break
				}
			}

			sendMessage (sender, outgoing) // Send for callback.

		case 1: // Relay to Lobby
		
		if sender.Lobby == nil{
			return; // no lobby. no relay.
		}

		outgoing.RelayToLobby = _OnP2P { Msg : netMessage.RelayToLobby.Msg }

		for e:=range sender.Lobby.Connections {
			if sender.Lobby.Connections[e] != nil{
				sendMessage (sender.Lobby.Connections[e], outgoing)
			}
		}
	}
}