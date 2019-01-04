<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="WebForm.index" %>

<!DOCTYPE html>
<html>
<head>
    <title>lol Lite</title>

    <script src="Scripts/jquery-1.6.4.min.js"></script>
    <script src="Scripts/jquery.signalR-2.2.0.min.js"></script>
    <script src="signalr/hubs"></script>
    <link href="css/css.css" rel="stylesheet" />
</head>
<body>
    <div id="kullaniciSec">
        Kullanıcı Adı Gir<br />
        <input id="kullaniciAdi" type="text" /><br />
        Karakter Seç Ve Başla<br />

        <img id="img10" src="img/1.0.png" />
        <img id="img11" src="img/1.1.png" style="display: none" />
        <img id="img12" src="img/1.2.png" style="display: none" />
        <img id="img20" src="img/2.0.png" />
        <img id="img21" src="img/2.1.png" style="display: none" />
        <img id="img22" src="img/2.2.png" style="display: none" />
        <img id="img30" src="img/3.0.png" />
        <img id="img31" src="img/3.1.png" style="display: none" />
        <img id="img32" src="img/3.2.png" style="display: none" />
        <img id="img40" src="img/4.0.png" />
        <img id="img41" src="img/4.1.png" style="display: none" />
        <img id="img42" src="img/4.2.png" style="display: none" />
        <img id="imgHeykel" src="img/heykel.png" style="display: none" />
    </div>

    <div id="oyuncular">

        <span class="mesajlar">Oyuncular.......</span>
        <br>
        <br>
        <span id="kl">-</span>
    </div>
    <canvas id="tuval" width="800" height="600">Tarayıcınız bu özelliği desteklemiyor.
    </canvas>
    <div id="mesajAlani">
        <div class="mesajlar">Mesajlar</div>
        <div id="mesajListesi"></div>
        <input id="mesajKutusu" type="text" />
    </div>

    <script>
        var lTuval = document.getElementById("tuval");
        var lImgHy = document.getElementById("imgHeykel");

        var lImg10 = document.getElementById("img10");
        var lImg11 = document.getElementById("img11");
        var lImg12 = document.getElementById("img12");
        var lImg20 = document.getElementById("img20");
        var lImg21 = document.getElementById("img21");
        var lImg22 = document.getElementById("img22");
        var lImg30 = document.getElementById("img30");
        var lImg31 = document.getElementById("img31");
        var lImg32 = document.getElementById("img32");
        var lImg40 = document.getElementById("img40");
        var lImg41 = document.getElementById("img41");
        var lImg42 = document.getElementById("img42");
        var lFirca = lTuval.getContext("2d");


        function KarakterCiz(aKullanici) {
            lFirca.beginPath();
            lFirca.shadowBlur = 0;
            //lFirca.shadowColor = "blue";
            var lKarakter = null;
            switch (aKullanici.KarakterImg) {
                case 10: lKarakter = lImg10; break;
                case 11: lKarakter = lImg11; break;
                case 12: lKarakter = lImg12; break;
                case 20: lKarakter = lImg20; break;
                case 21: lKarakter = lImg21; break;
                case 22: lKarakter = lImg22; break;
                case 30: lKarakter = lImg30; break;
                case 31: lKarakter = lImg31; break;
                case 32: lKarakter = lImg32; break;
                case 40: lKarakter = lImg40; break;
                case 41: lKarakter = lImg41; break;
                case 42: lKarakter = lImg42; break;
                default: lKarakter = null;
            }
            if (lKarakter != null) {
                lFirca.drawImage(lKarakter, aKullanici.KonumX - 40, aKullanici.KonumY - 40);
            }
            lFirca.drawImage(lImgHy, 425, 120);
            if (lKarakter != null) {
                KullaniciAdiYaz(aKullanici);
                KullaniciCaniCiz(aKullanici);
            }
        }

        function KullaniciAdiYaz(aKullanici) {
            lFirca.font = "bold 12px Arial";
            lFirca.shadowBlur = 5;
            lFirca.shadowColor = "#ffffff";
            lFirca.fillStyle = "#000000";
            lFirca.strokeStyle = "#CD0C0F";
            lFirca.textAlign = 'center';
            lFirca.fillText(aKullanici.KullaniciAdi, aKullanici.KonumX, aKullanici.KonumY - 60);
        }

        function KullaniciCaniCiz(aKullanici) {
            lFirca.shadowBlur = 0;
            lFirca.fillStyle = "#277498";
            lFirca.fillRect(aKullanici.KonumX - 40, aKullanici.KonumY - 56, Math.trunc(aKullanici.Can * 0.8), 8);
            lFirca.strokeStyle = "#0D1510";
            lFirca.lineWidth = 1;
            lFirca.strokeRect(aKullanici.KonumX - 41, aKullanici.KonumY - 56, 82, 8);
        }


        function AtesEt(aKullanici) {
            if (!aKullanici.AtesEder) {
                lFirca.beginPath();
                lFirca.shadowBlur = 10;
                lFirca.shadowColor = "#FBEB80";
                lFirca.fillStyle = "#FEFFFB";
                lFirca.strokeStyle = "#F2B25E";
                lFirca.arc(aKullanici.AtesKonumX, aKullanici.AtesKonumY, 4, 0, 2 * Math.PI);
                lFirca.fill();
                lFirca.stroke();
            }
        }

        var hubum = $.connection.lolHub;
        hubum.client.MesajGeldi = function (aMesaj) {
            $("#mesajListesi").append(aMesaj + "<br/>");
            document.getElementById('mesajListesi').scrollTop = document.getElementById('mesajListesi').scrollTopMax;
        };
        hubum.client.Alert = function (aMesaj) {
            alert(aMesaj);
        };

        hubum.client.EkraniCiz = function (aKullanicilar) {
            lFirca.clearRect(0, 0, lTuval.width, lTuval.height);
            $("#kl").html("");
            aKullanicilar.forEach(function (aKullanici) {
                $("#kl").append(aKullanici.Puan + " | " + aKullanici.KullaniciAdi + " ( " + aKullanici.Can + " )<br/>");
                AtesEt(aKullanici);
                KarakterCiz(aKullanici);
            });
        };

        $("#mesajKutusu").bind('keypress', function (e) {
            if (e.keyCode === 13) {
                hubum.server.mesajGondert($("#mesajKutusu").val());
                $("#mesajKutusu").val("");
            }
        });


        document.oncontextmenu = function () { return false; };
        $("#tuval").mousedown(function (e) {
            if (e.button === 2) {
                hubum.server.konumDegistir(e.pageX - $("#tuval").position().left, e.pageY - $("#tuval").position().top);
                return false;
            }
            return true;
        }).click(function (e) {
            hubum.server.atesEttir(e.pageX - $("#tuval").position().left, e.pageY - $("#tuval").position().top);
        });

        function Baglan(aKarakterId) {
            if ($("#kullaniciAdi").val().length < 3) {
                alert("Kullanıcı adı geçersiz..");
            } else {
                $.connection.hub.start().done(function () {
                    hubum.server.connectOl(aKarakterId, $("#kullaniciAdi").val());
                    $("#kullaniciSec").hide();
                });
            }
        }

        $("#img10").click(function () { Baglan(1); });
        $("#img20").click(function () { Baglan(2); });
        $("#img30").click(function () { Baglan(3); });
        $("#img40").click(function () { Baglan(4); });

        if ($.connection.hub.state === 1) {
            $("#kullaniciSec").hide();
        }
    </script>


</body>
</html>
