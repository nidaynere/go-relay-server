package main

/// Base network message
type OutgoingMessage struct {
	Purpose int `json:"t,omitempty"` // Message Purpose=> -1= Ping 0=JoinLobby, 1=RelayToLobby
	JoinLobby _OnJoinLobby `json:"jl,omitempty"` // Join lobby data, if exist
	RelayToLobby _OnP2P `json:"m,omitempty"` // Relay To Lobby data, if exist
	LobbyUpdate _OnLobbyUpdate `json:"lu,omitempty"` // Relay To Lobby data, if exist
} 

type _OnJoinLobby struct {
	Success bool  // Success
}

type _OnP2P struct {
	Msg string
}

type _OnLobbyUpdate struct {
	Members int
}