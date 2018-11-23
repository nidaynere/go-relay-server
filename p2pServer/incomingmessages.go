package main

/// Base network message
type IncomingMessage struct {
	Purpose int `json:"t,omitempty"` // Message Purpose=> 0=JoinLobby, 1=RelayToLobby
	RelayToLobby _RelayToLobby `json:"m,omitempty"` // Relay To Lobby data, if exist
} 

type _RelayToLobby struct {
	Msg []byte `json:"m,omitempty"` // This will be relayed to lobby. 
}