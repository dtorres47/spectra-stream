using System;
using System.Collections.Generic;

namespace SpectraStream.Api.Models
{
    public class PersistState
    {
        public List<QuestState> ActiveQuests { get; set; } = new();
        public List<RequestItem> RequestsPending { get; set; } = new();
        public List<RequestItem> RequestsActive { get; set; } = new();
        public List<TTSItem> TTSQueue { get; set; } = new();
        public int ReqSeq { get; set; }
        public int TTSSeq { get; set; }
        public long SavedAtUnix { get; set; }
    }
}