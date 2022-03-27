using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkLabLibrary.Models
{
	public class Session
	{
        public int SessionId { get; set; }
        
        public string SessionDate { get; set; }

        public string StartedAt { get; set; }

        public string EndedAt { get; set; }

        public int ParticipantCount { get; set; }

        public int FileId { get; set; }

        public int FileTypeId { get; set; }

        public string FileTitle { get; set; }

        public string FilePath { get; set; }

        public string Participants { get; set; }

    }
}
