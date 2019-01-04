using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace WebForm.Hubs
{
    public class LolHub : Hub
    {

        public void ConnectOl(int aKarakterId, string aKullaniciAdi)
        {
            Motor.HaritaDoldur();
            if (Motor.TumKullanicilar.Count >= 16)
            {
                Clients.Caller.alert("Üzgünüm, Yeterince Oyuncu Var, Oyunu izleyebilir oyunculardan birisi çıktığında oyuna girebilirsin.");
                return;
            }
            if (!Motor.TumKullanicilar.Any(k => k.HubId == Context.ConnectionId))
                Motor.TumKullanicilar.Add(new DtoKullanici(Context.ConnectionId, aKarakterId, aKullaniciAdi));

            Clients.All.mesajGeldi("<span class=\"sari\"><b>" + aKullaniciAdi + ",</b> Oyuna Bağlandı.</span>");
            Motor.Calistir(this);
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            var lCikisYapan = Motor.TumKullanicilar.FirstOrDefault(k => k.HubId == Context.ConnectionId);
            if (lCikisYapan != null)
            {
                lCikisYapan.CikisZamani = DateTime.Now;
                Clients.All.mesajGeldi("<span class=\"sari\"><b>" + lCikisYapan.KullaniciAdi + ",</b> Oyundan Ayrıldı.</span>");
            }
            return base.OnDisconnected(stopCalled);
        }

        public void MesajGondert(string aMesaj)
        {
            var lKullanici = Motor.Kullanicilar.FirstOrDefault(k => k.HubId == Context.ConnectionId);
            var lKullaniciAdi = "";
            if (lKullanici != null)
                lKullaniciAdi = lKullanici.KullaniciAdi;
            Clients.All.mesajGeldi("<b>" + lKullaniciAdi + " : </b>" + aMesaj);
        }

        public void KonumDegistir(int x, int y)
        {
            if (Motor.DuvaraGeldiMi(x, y))
                return;
            var lKullanici = Motor.Kullanicilar.FirstOrDefault(k => k.HubId == Context.ConnectionId);
            if (lKullanici != null && lKullanici.Can > 0)
            {
                lKullanici.HedefX = x;
                lKullanici.HedefY = y;
            }
        }
        public void AtesEttir(int x, int y)
        {
            var lKullanici = Motor.Kullanicilar.FirstOrDefault(k => k.HubId == Context.ConnectionId);
            if (lKullanici != null && lKullanici.AtesEder && lKullanici.Can > 0)
            {
                lKullanici.AtesEder = false;
                lKullanici.AtesOmur = 200;
                lKullanici.AtesKonumX = lKullanici.KonumX;
                lKullanici.AtesKonumY = lKullanici.KonumY;
                lKullanici.AtesHedefX = x;
                lKullanici.AtesHedefY = y;

                var lFarkX = Math.Abs(lKullanici.AtesHedefX - lKullanici.AtesKonumX);
                var lFarkY = Math.Abs(lKullanici.AtesHedefY - lKullanici.AtesKonumY);
                var lBuyuk = lFarkX > lFarkY ? lFarkX : lFarkY;
                lKullanici.AtesArtisX = lFarkX / lBuyuk;
                lKullanici.AtesArtisY = lFarkY / lBuyuk;
                if (lKullanici.AtesHedefX < lKullanici.AtesKonumX)
                    lKullanici.AtesArtisX = decimal.Negate(lKullanici.AtesArtisX);

                if (lKullanici.AtesHedefY < lKullanici.AtesKonumY)
                    lKullanici.AtesArtisY = decimal.Negate(lKullanici.AtesArtisY);
            }
        }
    }

    public static class Motor
    {
        public static void HaritaDoldur()
        {
            if (_Duvarlar.Any())
                return;
            var lImage = new Bitmap(@"D:\OneDrive\lol\lolApp\WebForm\img\zemin.png");
            for (var y = 0; y < lImage.Height; y++)
                for (var x = 0; x < lImage.Width; x++)
                {
                    var lRenk = lImage.GetPixel(x, y);
                    if (lRenk.A == 255)
                        _Duvarlar.Add(new Kordinat(x, y));
                    else if (lRenk.A > 0)
                        _Cimenler.Add(new Kordinat(x, y));
                }
        }

        const int YariCap = 40;
        public static List<DtoKullanici> TumKullanicilar = new List<DtoKullanici>();
        public static List<DtoKullanici> Kullanicilar
        {
            get { return TumKullanicilar.Where(x => x.CikisZamani == null).ToList(); }
        }
        private static List<Kordinat> _Duvarlar = new List<Kordinat>();
        private static List<Kordinat> _Cimenler = new List<Kordinat>();

        public static bool DuvaraGeldiMi(decimal x, decimal y)
        {
            return _Duvarlar.Any(d => d.X == Math.Round(x) && d.Y == Math.Round(y));
        }
        public static bool CimeneGeldiMi(decimal x, decimal y)
        {
            return _Cimenler.Any(d => d.X == Math.Round(x) && d.Y == Math.Round(y));
        }

        public static LolHub Hubum;
        public static void Calistir(LolHub aHub)
        {
            if (Hubum != null) return;
            Hubum = aHub;
            new Thread(EkraniCiz).Start();
            new Thread(HareketleriHesapla).Start();
            new Thread(AtesIlerlet).Start();
            new Thread(CanlariTakipEt).Start();
        }
        public static void CikanlariSil()
        {
            while (true)
            {
                TumKullanicilar.RemoveAll(x => x.CikisZamani < DateTime.Now.AddSeconds(-3));
                Thread.Sleep(5000);
            }
        }
        public static void EkraniCiz()
        {
            while (true)
            {
                VurulanlariHesapla();
                var lEkranaGidecekler = GonderilecekBilgieriGetir();

                for (var i = Kullanicilar.Count - 1; i >= 0; i--)
                {
                    foreach (var lGidecek in lEkranaGidecekler)
                        lGidecek.KarakterImgHesapla(Kullanicilar[i].HubId);
                    if (Kullanicilar.Any())
                        Hubum.Clients.Client(Kullanicilar[i].HubId).ekraniCiz(lEkranaGidecekler);
                }
                Thread.Sleep(35);
            }
        }
        public static List<Kullanici> GonderilecekBilgieriGetir()
        {
            var lResult = new List<Kullanici>();
            foreach (var lKullanici in Kullanicilar)
            {
                var lYeni = new Kullanici();
                foreach (var lHedefPro in lYeni.GetType().GetProperties())
                {
                    var lKaynakPro = lKullanici.GetType().GetProperties().FirstOrDefault(p => p.Name == lHedefPro.Name);
                    if (lKaynakPro != null && lKaynakPro.PropertyType == lHedefPro.PropertyType)
                        lHedefPro.SetValue(lYeni, lKaynakPro.GetValue(lKullanici, null));
                }
                lYeni.Can = Math.Round(lYeni.Can);
                lResult.Add(lYeni);
            }
            return lResult;
        }
        public static void VurulanlariHesapla()
        {
            foreach (var lKullanici in Kullanicilar)
            {
                if (!lKullanici.AtesEder)
                {
                    if (DuvaraGeldiMi(lKullanici.AtesKonumX, lKullanici.AtesKonumY))
                    {
                        lKullanici.AtesOmur = 0;
                        lKullanici.AtesEder = true;
                    }
                    else
                    {
                        var lVurulanKullanicilar = Kullanicilar.Where(x =>
                                    x.HubId != lKullanici.HubId
                                    && x.Can > 0
                                    && Math.Abs(x.KonumX - lKullanici.AtesKonumX) < YariCap
                                    && Math.Abs(x.KonumY - lKullanici.AtesKonumY) < YariCap
                                    && (Math.Abs(x.KonumX - lKullanici.AtesKonumX) + Math.Abs(x.KonumY - lKullanici.AtesKonumY)) < YariCap
                                    ).ToList();
                        if (lVurulanKullanicilar.Any())
                        {
                            lKullanici.Puan += 1;
                            lKullanici.AtesOmur = 0;
                            lKullanici.AtesEder = true;
                        }
                        foreach (var lVurulan in lVurulanKullanicilar)
                        {
                            lVurulan.Can -= 7;
                            if (lVurulan.Can <= 0 && lVurulan.CanlanmayaKalan <= 0)
                            {
                                lVurulan.CanlanmayaKalan = 100;
                                lVurulan.Can = 0;
                                lKullanici.Puan += 10;
                            }
                        }
                    }
                }
            }

        }

        public static void CanlariTakipEt()
        {
            while (true)
            {
                foreach (var lKullanici in Kullanicilar.Where(x => x.Can <= 0))
                {
                    lKullanici.CanlanmayaKalan -= 1;
                    if (lKullanici.CanlanmayaKalan <= 0)
                    {
                        lKullanici.CanDoldurRasgeleKonumlandir();
                    }
                }
                foreach (var lKullanici in Kullanicilar)
                {
                    if (lKullanici.Can > 0)
                        lKullanici.Can += 0.1m;
                }
                Thread.Sleep(100);
            }
        }
        public static void AtesIlerlet()
        {
            while (true)
            {
                foreach (var lKullanici in Kullanicilar)
                {
                    lKullanici.AtesEder = lKullanici.AtesOmur <= 0;
                    if (lKullanici.AtesEder) continue;
                    lKullanici.AtesOmur -= 1;
                    lKullanici.AtesKonumX += lKullanici.AtesArtisX * 2;
                    lKullanici.AtesKonumY += lKullanici.AtesArtisY * 2;
                }
                Thread.Sleep(5);
            }
        }
        public static void HareketleriHesapla()
        {
            while (true)
            {
                foreach (var lKullanici in Kullanicilar)
                {
                    var lEskiKonumX = lKullanici.KonumX;
                    var lEskiKonumY = lKullanici.KonumY;

                    var lMesafeX = Math.Abs(lKullanici.HedefX - lKullanici.KonumX);
                    var lMesafeY = Math.Abs(lKullanici.HedefY - lKullanici.KonumY);
                    var lEnUzakM = lMesafeX > lMesafeY ? lMesafeX : lMesafeY;


                    if (lEnUzakM < 1)
                    {
                        lKullanici.KonumX = lKullanici.HedefX;
                        lKullanici.KonumY = lKullanici.HedefY;
                    }
                    else
                    {
                        decimal lArtisX = lMesafeX / lEnUzakM;
                        decimal lArtisY = lMesafeY / lEnUzakM;
                        lKullanici.KonumX += lArtisX * (lKullanici.HedefX > lKullanici.KonumX ? 1 : -1);
                        lKullanici.KonumY += lArtisY * (lKullanici.HedefY > lKullanici.KonumY ? 1 : -1);
                    }
                    if (DuvaraGeldiMi(lKullanici.KonumX, lKullanici.KonumY))
                    {
                        lKullanici.KonumX = lEskiKonumX;
                        lKullanici.KonumY = lEskiKonumY;
                        lKullanici.HedefX = lEskiKonumX;
                        lKullanici.HedefY = lEskiKonumY;
                    }

                }
                Thread.Sleep(10);
            }
        }


    }
    public class Kullanici
    {
        public string HubId { get; set; }

        public int KarakterImg { get; set; }
        public int KarakterId { get; set; }
        public string KullaniciAdi { get; set; }
        public decimal KonumX { get; set; }
        public decimal KonumY { get; set; }
        public decimal AtesKonumX { get; set; }
        public decimal AtesKonumY { get; set; }
        public bool AtesEder { get; set; }
        public int Puan { get; set; }
        private decimal _can;
        public decimal Can
        {
            get { return _can; }
            set
            {
                if (value < 0)
                    _can = 0;
                else if (value > 100)
                    _can = 100;
                else
                    _can = value;
            }
        }

        public bool CimendeMi()
        {
            return Motor.CimeneGeldiMi(KonumX, KonumY);
        }
        public void KarakterImgHesapla(string aHubId)
        {
            KarakterImg = KarakterId * 10;
            if (CimendeMi())
                if (HubId == aHubId)
                    if (Can == 0)
                        KarakterImg += 1;
                    else
                        KarakterImg += 2;
                else
                    KarakterImg = 0;
            else if (Can == 0)
                KarakterImg += 1;
        }
    }
    public class DtoKullanici : Kullanici
    {
        public DateTime? CikisZamani { get; set; }
        private const int EkranGenislik = 800;
        private const int EkranYukseklik = 600;
        public void CanDoldurRasgeleKonumlandir()
        {
            var lRandeom = new Random();
            decimal lX;
            decimal lY;
            do
            {
                lX = lRandeom.Next(EkranGenislik);
                lY = lRandeom.Next(EkranYukseklik);
            } while (Motor.DuvaraGeldiMi(lX, lY));
            this.Can = 100;
            this.KonumX = lX;
            this.KonumY = lY;
            this.HedefX = lX;
            this.HedefY = lY;
        }
        public decimal HedefX { get; set; }
        public decimal HedefY { get; set; }
        public int AtesHedefX { get; set; }
        public int AtesHedefY { get; set; }
        public int AtesOmur { get; set; }
        public decimal AtesArtisX { get; set; }
        public decimal AtesArtisY { get; set; }
        public int CanlanmayaKalan { get; set; }

        public DtoKullanici(string aHubId, int aKarakterId, string aKullaniciAdi)
        {
            CikisZamani = null;
            HubId = aHubId;
            KarakterId = aKarakterId;
            KullaniciAdi = aKullaniciAdi;
            AtesEder = true;
            Puan = 0;
            CanlanmayaKalan = 0;
            CanDoldurRasgeleKonumlandir();
        }
    }

    public class Kordinat
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Kordinat(int x, int y)
        {
            X = x;
            Y = y;
        }
    }


}