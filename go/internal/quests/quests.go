package quests

import "github.com/go-chi/chi/v5"

type QuestState struct {
    ID string
}

func RegisterRoutes(r chi.Router) {}
func ListActiveQuests() []QuestState { return nil }
func SetState(qs []QuestState) {}