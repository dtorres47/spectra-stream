package ws

type WSMsg struct {
    Type string
    Data interface{}
}

func Broadcast(msg WSMsg) {}
func ClientsCount() int { return 0 }
func WSHandler(w http.ResponseWriter, r *http.Request) {}