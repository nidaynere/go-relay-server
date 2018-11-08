///TODO need parser to split to handle sent string

package main

import (
    "fmt"
    "net"
    "os"
	"time"
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
			fmt.Println("ping to connection")
			
			// Send a response back to person contacting us.
			_, err := connections[i].conn.Write([]byte("ping"))
			
			if err != nil {
				connections [i].isConnected = false
				fmt.Println("Client disconnected", connections [i].isConnected)
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
					fmt.Println("incoming data: ", incoming) 
					//fmt.Println("sender's lobby: ", connections[i].lobby)
					go sendMessage (&connections[i], "mesajini aldim kankey")
					// TODO: Relay the message to all lobby members
				}
		}
	}			
}

//usage: sendMessage (&connections[i], incoming) // send back the incoming message
func sendMessage (conn *connection, msg string) {
		// Send a response back to person contacting us.
		conn.conn.Write([]byte(msg))
}