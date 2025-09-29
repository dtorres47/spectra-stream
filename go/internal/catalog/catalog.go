package catalog

import "github.com/go-chi/chi/v5"

func LoadCatalog() {}
func GetQuest(id string) (Quest, bool) { return Quest{}, false }
func RegisterRoutes(r *chi.Mux)       {}

type Quest struct {
    ID string
}