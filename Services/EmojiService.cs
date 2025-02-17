using System.Collections.Generic;

namespace Coflnet.Sky.Chat.Services
{
    public class EmojiService
    {
        Dictionary<string, string> Emoji = new(){
            {":tableflip:","§4(ノಠ益ಠ)§7ノ彡┻━┻"},
            {":sad:", "☹"},
            {":smile:", "☺"},
            {":grin:", "ツ"},
            {":heart:", "♡"},
            {":skull:", "☠"},
            {":airplane:", "✈"},
            {":check:", "§2✔"},
            {"<3", "§c❤"},
            {":star:", "§6✮"},
            {":yes:", "§a✔"},
            {":no:", "§c✖"},
            {":java:", "§b☕"},
            {":arrow", "§e➜"},
            {":shrug:", @"§e¯\_(ツ)_/¯"},
            {"o/", "§d( ﾟ◡ﾟ)/"},
            {":123:", "§a1§e2§c3"},
            {":totem:", "§b☉§e_§b☉"},
            {":typing:", "§e✎§6..."},
            {":maths:", "§a√§e(§aπ+x§e)§a=§cL"},
            {":snail:", "§e@§b'§e-§b'"},
            {":thinking:", "§6(§a0§6.§ao§c?§6)"},
            {":gimme:", "§6༼つ◕_◕༽つ"},
            {":wizard:", "§e(§b'§e-§b'§e)⊃━§c☆ﾟ§d.*･｡ﾟ"},
            {":pvp:", "§e⚔"},
            {":peace:", "§e✌"},
            {":oof:", "§cOOF"},
            {":puffer:", "§e<('O')>"},
            {":yey:", "§aヽ (◕◡◕) ﾉ"},
            {":cat:", "§e= §b＾● ⋏ ●＾ §e="},
            {":dab:", "§d<§eo§d/"},
            {":dj:", "§1ヽ§5(§d⌐§c■§6_§e■§b)§3ノ§9♬"},
            {":snow:", "§b☃"},
            {":^_^:", "§a^_^"},
            {"h/", "§eヽ(^◇^*)/"},
            {":^-^:", "§a^-^"},
            {":sloth:", "§6(§8・§6⊝§8・§6)"},
            {":cute:", "§e(§a✿§e◠‿◠)"},
            {":dog:", "§6(ᵔᴥᵔ)"},
            {":fyou:", "§6┌∩┐(§4◣§c_§4◢§6)┌∩┐"},
            {":angwyflip:", "§c(ノಠ益ಠ)ノ§f彡§7┻━┻"},
            {":snipe:", "§e︻デ═一"},
            {":cofl:", "§1C§6ofl§f"},
            {":preapi:", "§cpre api§f"},
            {":tm:", "§c™"},
            {":r:", "§c®"},
            {":c:", "§c©"},
            {":crown:", "§e♔"},
            {":fire:", "§c♨"},
            {":sword:", "§c⚔"},
            {":shield:", "§c🛡"},
            {":cross:", "§c✝"},
            {":star1:", "§c✪"},
            {":star2:", "§c✫"},
            {":star3:", "§c✬"},
            {":star4:", "§c✭"},
            {":star5:", "§c✮"},
            {":star6:", "§c✯"},
            {":star7:", "§c✰"},
            {":star8:", "§c✱"},
            {":star9:", "§c✲"},
            {":star10:", "§c✳"},
            {":star11:", "§c✴"},
            {":star12:", "§c✵"},
            {":star13:", "§c✶"},
            {":star14:", "§c✷"},
            {":star15:", "§c✸"},
            {":star16:", "§c✹"},
            {":star17:", "§c✺"},
            {":star18:", "§c✻"},
            {":star19:", "§c✼"},
            {":star20:", "§c✽"},
            {":star21:", "§c✾"},
            {":star22:", "§c✿"},
            {":star23:", "§c❀"},
            {":star24:", "§c❁"},
            {":star25:", "§c❂"},
            {":star26:", "§c❃"},
            {":star27:", "§c❄"},
            {":star28:", "§c❅"},
            {":star29:", "§c❆"},
            {":star30:", "§c❇"},
            {":star31:", "§c❈"},
            {":star32:", "§c❉"},
            {":star33:", "§c❊"},
            {":star34:", "§c❋"}
        };

        public string ReplaceIn(string value)
        {
            foreach (var item in Emoji)
            {
                if (!value.Contains(item.Key))
                    continue;
                value = value.Replace(item.Key, item.Value);
            }
            return value;
        }
    }
}
