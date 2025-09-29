package requests

import "github.com/go-chi/chi/v5"

type RequestItem struct {
    ID int
}

func RegisterRoutes(r chi.Router) {}
func GetPendingRequests() []*RequestItem { return nil }
func GetActiveRequests() []*RequestItem  { return nil }
func GetNextID() int                     { return 0 }
func SetState(pending, active []*RequestItem, seq int) {}