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
	Name string // Lobby name
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

			var additional string
			if Connections[i].Lobby != nil{
				additional = " ,lobby is: " + Connections[i].Lobby.Name
			}

			_, err := Connections[i].Conn.Write([]byte("->ping" + additional))
			
			if err != nil {
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
func sendMessage (conn *connection, msg string) {
		// Send a response back to person contacting us.
		conn.Conn.Write([]byte(msg))
}

/// Base network message
type NetworkMessage struct {
	Purpose int `json:"t,omitempty"` // Message Purpose=> 0=CreateLobby, 1=JoinLobby, 2=RequestLobbies, 4=RelayToLobby
	CreateLobby _CreateLobby `json:"cl,omitempty"` // Create lobby data, if exist
	JoinLobby _JoinLobby `json:"jl,omitempty"` // Join lobby data, if exist
	RequestLobbies _RequestLobbies `json:"rl,omitempty"` // Request Lobbies data, if exist
	RelayToLobby _RelayToLobby `json:"m,omitempty"` // Relay To Lobby data, if exist
} 

type _CreateLobby struct {
	LobbyName string `json:"n,omitempty"` // Name of the lobby.
}

type _JoinLobby struct {
	Index int `json:"i,omitempty"` // Index of the lobby.
}

type _RelayToLobby struct {
	Msg string `json:"m,omitempty"` // This will be relayed to lobby. 
}

type _RequestLobbies struct {
	Page int `json:"p,omitempty"`// Page
}

///Process the incoming message, and relay the message to lobby if the connection is in a lobby.
func ProcessData (data string, sender *connection) {
	var netMessage NetworkMessage
	if err := json.NewDecoder(strings.NewReader(data)).Decode(&netMessage); err != nil {
			/// Message cannot be parsed.
		return
	}

	fmt.Println ("incoming message purpose: ", netMessage.Purpose)

	switch netMessage.Purpose {
		case 0: // Create Lobby
			if sender.Lobby != nil{
				fmt.Println ("Create lobby request: but already in a lobby. Leave it first.")
				return;
			}

			fmt.Println ("CreateLobby with name: ", netMessage.CreateLobby.LobbyName)

			targetLobby := -1
			for i := range Lobbies {
				if AnyPlayerInLobby (&Lobbies[i]) == false {	
					targetLobby = i
					break
				}
			}

			if targetLobby == -1{
				sendMessage (sender, "Cannot create a lobby because server got maximum lobby:(")
				return
			}

			Lobbies[targetLobby].Name = netMessage.CreateLobby.LobbyName
			Lobbies[targetLobby].Connections[0] = sender
			sender.Lobby = &Lobbies[targetLobby]

			sendMessage (sender, "Lobby created succesfully with name: " + sender.Lobby.Name)

		case 1: // Join Lobby
			if sender.Lobby != nil{
				fmt.Println ("Join lobby request: but already in a lobby. Leave it first.")
				return;
			}

			fmt.Println ("Join lobby request: ", netMessage.JoinLobby.Index)

			var tLobby lobby = Lobbies[netMessage.JoinLobby.Index]

			if AnyPlayerInLobby (&tLobby) == false {
				sendMessage (sender, "Lobby is not found.")
				fmt.Println ("Lobby is not found. ", netMessage.JoinLobby.Index)
				return
			}

			joinable, index := GetLobbySlot (&tLobby)
			if !joinable {
				sendMessage (sender, "Lobby is full")
				fmt.Println ("Lobby is full ", netMessage.JoinLobby.Index)
				return
			}
			
			tLobby.Connections[index] = sender
			sender.Lobby = &tLobby

			sendMessage (sender, "Successfully joined to lobby " + tLobby.Name)
		case 2: // Request Lobbies
			if sender.Lobby != nil{
				fmt.Println ("Request lobby request: but already in a lobby. Leave it first.")
				return;
			}
		case 3: // Relay to Lobby
	}
}