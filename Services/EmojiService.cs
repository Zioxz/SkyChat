using System.Collections.Generic;

namespace Coflnet.Sky.Chat.Services
{
    public class EmojiService
    {
        Dictionary<string, string> Emoji = new(){
            {"tableflip","(ノಠ益ಠ)ノ彡┻━┻"},
            {"sad", "☹"},
            {"smile", "☺"},
            {"grin", "ツ"},
            {"heart", "♡"},
            {"skull", "☠"},
            {"airplane", "✈"},
            {"check", "✔"},
        };

        public string ReplaceIn(string value)
        {
            foreach (var item in Emoji)
            {
                if(!value.Contains(item.Key))
                    continue;
                value = value.Replace(':' + item.Key + ':', item.Value);
            }
            return value;
        }
    }
}
