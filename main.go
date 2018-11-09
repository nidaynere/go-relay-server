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
	conn net.Conn
	isConnected bool
	lobby int // index of the registered lobby.
}

///Lobby
type lobby struct {
	connections [8]*connection // A lobby can hold maximum 8 players.
	name string // Lobby name
}

/// ALL CONNECTIONS
var connections [8000]connection // Maximum 8000 players
var lobbies [1000]lobby // In 1000 lobbies

func addconnection (newconnection net.Conn) {
	for i := range connections {
		if !connections[i].isConnected {
			connections[i].conn = newconnection
			connections[i].isConnected = true
			fmt.Println ("Client connected")
			return
		}
	}
	
	fmt.Println("Server is full!")
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
	for i := range connections {
		if connections[i].isConnected {
			// Ping the connection.
			_, err := connections[i].conn.Write([]byte("->ping"))
			
			if err != nil {
				connections [i].isConnected = false
				fmt.Println("Client disconnected")
				continue
			}
		}
	}
}

func handleMessages () {
	for i := range connections {
		if connections[i].isConnected {
				//Make a buffer to hold incoming data.
				buf := make([]byte, 1024)
				//Read the incoming connection into the buffer.
				length, error := connections[i].conn.Read(buf)

				if error == nil {
					var incoming string = string(buf[:length])
					//fmt.Println("sender's lobby: ", connections[i].lobby)
					//go sendMessage (&connections[i], "mesajini aldim kankey")
					// TODO: Relay the message to all lobby members
					ProcessData (incoming, &connections[i])
				}
		}
	}			
}

//usage: sendMessage (&connections[i], incoming) // send back the incoming message
func sendMessage (conn *connection, msg string) {
		// Send a response back to person contacting us.
		conn.conn.Write([]byte(msg))
}

/// Base network message
type NetworkMessage struct {
	Purpose int `json:"t,omitempty"` // Message Purpose=> 0=CreateLobby, 1=JoinLobby, 2=RequestLobbies, 4=RelayToLobby
	CreateLobby CreateLobby `json:"cl,omitempty"` // Create lobby data, if exist
	JoinLobby JoinLobby `json:"jl,omitempty"` // Join lobby data, if exist
	RequestLobbies RequestLobbies `json:"rl,omitempty"` // Request Lobbies data, if exist
	RelayToLobby RelayToLobby `json:"m,omitempty"` // Relay To Lobby data, if exist
} 

type CreateLobby struct {
	name string // Name of the lobby.
}

type JoinLobby struct {
	index int // Index of the lobby.
}

type RelayToLobby struct {
	data string // This will be relayed to lobby. 
}

type RequestLobbies struct {
	page int // Page
}

///Process the incoming message, and relay the message to lobby if the connection is in a lobby.
func ProcessData (data string, sender *connection) {
	var netMessage NetworkMessage
	if err := json.NewDecoder(strings.NewReader(data)).Decode(&netMessage); err != nil {
			/// Message cannot be parsed.
		return
	}

	//TODO Switch case via netMessage.Purpose
}