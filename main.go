//TODO--> Lobby struct. Lobby list.

package main

import (
    "fmt"
    "net"
    "os"
	"time"
)

var l net.Listener = nil

const (
    CONN_HOST = "localhost"
    CONN_PORT = "3333"
    CONN_TYPE = "tcp"
)

type connection struct {
	conn net.Conn
	isConnected bool
}

// Max connections: 16
var connections [16]connection

func addconnection (newconnection net.Conn) {
	fmt.Println("New connection")
	for i, conn := range connections {
		if !conn.isConnected {
			connections[i].conn = newconnection
			connections[i].isConnected = true
			break;
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
	
	l = r
	
    fmt.Println("Listening on " + CONN_HOST + ":" + CONN_PORT)

	tick := time.Tick(1 * time.Second)

    for {
        select {
        case <-tick:
			go Listen()
			go ConnectionControl()
			// -> TODO Message reader.
		}
    }
}

/// Listen for connections
func Listen () {
	conn, err := l.Accept()

	if err != nil {
		fmt.Println("Error accepting: ", err.Error())
		//os.Exit(1)
	} else {
	// New connection here.
		addconnection (conn)
	}
}

// Handles the connections & drops
func ConnectionControl() {
	for i := range connections {
		if connections[i].isConnected {
			// Make a buffer to hold incoming data.
			//buf := make([]byte, 1024)
			// Read the incoming connection into the buffer.
			//_, err := conn.conn.Read(buf)
			
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