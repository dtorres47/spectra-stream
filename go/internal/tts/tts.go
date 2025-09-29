package tts

import "github.com/go-chi/chi/v5"

type TTSItem struct {
    ID int
}

func RegisterRoutes(r *chi.Mux) {}
func GetQueue() []*TTSItem { return nil }
func GetNextID() int { return 0 }
func SetState(queue []*TTSItem, seq int) {}