using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WazeBotDiscord.Announce
{
    public class AnnounceModal : IModal
    {
        public string Title => "Send Announcement";

        [InputLabel("Message")]
        [ModalTextInput("message", TextInputStyle.Paragraph, "Enter your announcement...", maxLength: 2000)]
        public string Message { get; set; }
    }
}
