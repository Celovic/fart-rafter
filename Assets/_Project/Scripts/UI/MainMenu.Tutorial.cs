using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FartRafters
{
    public partial class MainMenu
    {
        Text homePlay;
        Text homeOnline;
        Text homeBoard;
        Text homeSettings;
        RectTransform practiceCard;
        RectTransform adventureCard;
        Text setupStart;

        static RectTransform Of(Component component)
        {
            if (component == null)
            {
                return null;
            }
            var rect = component.transform as RectTransform;
            return component is Text && rect != null && rect.parent is RectTransform parent && parent.GetComponent<Button>() != null ? parent : rect;
        }

        void ShowTutorial(RectTransform screen)
        {
            if (TutorialGuide.Active || screen == null)
            {
                return;
            }
            string id = null;
            var steps = new List<TutorialGuide.Step>();
            if (screen == homeScreen)
            {
                id = "home";
                steps.Add(new TutorialGuide.Step(Of(homePlay), "Fart Rafter'a hoş geldin! Osurarak salı yönet, bitiş bayrağına ilk sen ulaş. Oyna'ya basınca Alıştırma ya da Macera'yı seçersin."));
                steps.Add(new TutorialGuide.Step(Of(homeOnline), "Online: oda kur, 6 haneli kodu arkadaşlarına ver; kodu giren her yerden aynı sala biner."));
                steps.Add(new TutorialGuide.Step(friendsButton, "Arkadaşlar: isimle arkadaş bul, gelen istekleri onayla, onları odana davet et."));
                steps.Add(new TutorialGuide.Step(Of(homeBoard), "Puan Listesi: tüm zamanlar, haftalık ve arkadaş sıralaması."));
                steps.Add(new TutorialGuide.Step(Of(homeSettings), "Ayarlar: isim, grafik kalitesi, yönlendirme (joystick ya da jiroskop) ve öğreticileri sıfırlama."));
                steps.Add(new TutorialGuide.Step(questsBadge != null ? (RectTransform)questsBadge.transform.parent : null, "Görevler: her gün 3 yeni görev ve giriş ödülü. Tamamladıkça puan kazanırsın."));
                steps.Add(new TutorialGuide.Step(Of(totalText), "Her oyunda puan kazanırsın; toplam puanın burada birikir."));
            }
            else if (screen == modeScreen)
            {
                id = "modes";
                steps.Add(new TutorialGuide.Step(practiceCard, "Alıştırma: 5 kısa, sabit harita. Kontrolleri öğrenmek için ideal; bir haritayı bitirince sonraki açılır."));
                steps.Add(new TutorialGuide.Step(adventureCard, "Macera: her oynayışta yepyeni bir parkur. Kolay, Orta, Zor ve Uzman zorlukları var."));
            }
            else if (screen == difficultyScreen && difficultyBest != null && difficultyBest.Length > 0)
            {
                id = "difficulty";
                steps.Add(new TutorialGuide.Step(difficultyBest[0].transform.parent as RectTransform, "Zorluk seç. Zorluk arttıkça parkur uzar, timsahlar hızlanır, akıntılar güçlenir ve puan çarpanı büyür."));
                steps.Add(new TutorialGuide.Step((RectTransform)null, "Saldaki kişi (oyuncu + bot) azaldıkça parkur biraz kolaylaşır: 4 kişide tam zorluk, 1 kişide %70."));
            }
            else if (screen == levelScreen && cards != null && cards.Length > 0)
            {
                id = "levels";
                steps.Add(new TutorialGuide.Step(cards[0].background.rectTransform, "Bir harita seç. Kilitli haritalar öncekini bitirince açılır; en iyi puanın kartın altında yazar."));
            }
            else if (screen == setupScreen)
            {
                id = "setup";
                steps.Add(new TutorialGuide.Step(slots[0].next, "Okla avatarını değiştir. Her avatarın kendine özgü osuruk sesi var."));
                steps.Add(new TutorialGuide.Step(Of(slots[0].costume), "Kostüm: her avatarın ücretsiz kostümleri var, istediğini giydir."));
                steps.Add(new TutorialGuide.Step(Of(playersText), "Aynı telefonda 1-4 kişi oynayabilir; ekran oyunculara bölünür."));
                steps.Add(new TutorialGuide.Step(Of(botCountText), "Boş koltuklara bot ekleyebilirsin; bot seviyesini yanındaki düğmeden seç."));
                steps.Add(new TutorialGuide.Step(Of(biomeText), "Harita: Nehir, Kutup (buzda sal kayar), Okyanus (girdaplara dikkat), Eğlence (havada kaydıraklar) ya da Rastgele."));
                steps.Add(new TutorialGuide.Step(Of(raftText), "Sal seç: her teknenin küçük farkları var. Dairelerin dolu olanı dayanıklılığı gösterir: dayanıklı sal çarpışmada daha az hasar alır ve daha zor parçalanır."));
                steps.Add(new TutorialGuide.Step(Of(setupStart), "Hazırsan Başla!"));
            }
            else if (screen == onlineScreen)
            {
                id = "online";
                steps.Add(new TutorialGuide.Step((RectTransform)null, "Online oyunda odada en fazla 4 kişi salı birlikte yönetir; fazlası izleyici olur."));
                steps.Add(new TutorialGuide.Step((RectTransform)null, "Oda Kur: modu ve haritayı seçersin, ekranda 6 haneli kod çıkar. Koda Katıl: arkadaşının kodunu girersin."));
            }
            else if (screen == lobbyScreen)
            {
                id = "lobby";
                steps.Add(new TutorialGuide.Step(Of(lobbyCode), "Bu kodu arkadaşlarına söyle; kodla odaya katılırlar."));
                steps.Add(new TutorialGuide.Step(Of(mapText), "Parkur, harita, botlar ve sal türünü yalnızca odayı kuran seçer."));
                steps.Add(new TutorialGuide.Step(Of(readyText), "Herkes Hazırım dediğinde oda kurucusu oyunu başlatır."));
            }
            else if (screen == friendsScreen)
            {
                id = "friends";
                steps.Add(new TutorialGuide.Step((RectTransform)null, "Arkadaş Bul ile isimle ara ve istek gönder. Karşı taraf onaylamadan arkadaş olunmaz."));
            }
            else if (screen == boardScreen)
            {
                id = "board";
                steps.Add(new TutorialGuide.Step((RectTransform)null, "Puan listesinde tüm zamanlar, haftalık ve arkadaşların arasındaki sıranı görürsün."));
            }
            if (id != null)
            {
                TutorialGuide.Show(canvasRect, id, steps);
            }
        }
    }
}
