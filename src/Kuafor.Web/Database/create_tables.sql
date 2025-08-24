CREATE TABLE Kullanicilar
(
    kullanici_id INT PRIMARY KEY identity(1,1),
    kullanici_ad Nvarchar(50) NOT NULL,
    kullanici_soyad Nvarchar(50) NOT NULL,
    kullanici_email Nvarchar(50) NOT NULL UNIQUE,
    kullanici_telno Nvarchar(20) NOT NULL UNIQUE,
    kullanici_sifre_hash Nvarchar(255) NOT NULL,
    kullanici_rol Nvarchar(10) NOT NULL CHECK (kullanici_rol IN ('Musteri', 'Calisan', 'Yonetici', 'Admin')),
    kullanici_sadakat_puani INT Default 0,
    kullanici_komisyon_orani DECIMAL(5, 2) NULL,
    kullanici_noshow_sayisi INT Default 0,
    kullanici_on_odeme_gerekli BIT DEFAULT 0,
    kullanici_olusturma_tarihi DATETIME2 DEFAULT GETDATE()
);

CREATE TABLE Hizmetler
(
    hizmet_id INT PRIMARY KEY identity(1,1),
    hizmet_ad NVARCHAR(100) NOT NULL,
    hizmet_aciklama NVARCHAR(MAX),
    hizmet_fiyat DECIMAL (10,2) NOT NULL,
    hizmet_sure_dk INT NOT NULL,
    hizmet_tip NVARCHAR(10) NOT NULL CHECK (hizmet_tip IN ('Hizmet', 'Paket', 'Urun'))
);

CREATE TABLE Hizmet_Paket_Icerikleri
(
    paket_hizmet_id INT,
    alt_hizmet_id INT,
    PRIMARY KEY (paket_hizmet_id, alt_hizmet_id),
    FOREIGN KEY (paket_hizmet_id) REFERENCES Hizmetler(hizmet_id) ON DELETE CASCADE,
    FOREIGN KEY (alt_hizmet_id) REFERENCES Hizmetler(hizmet_id)
)

CREATE TABLE Fiyatlandirma_Kurallari
(
    kural_id INT PRIMARY KEY IDENTITY(1,1),
    hizmet_id INT,
    kural_gun NVARCHAR(15) CHECK (kural_gun IN ('Pazartesi', 'Sali', 'Carsamba', 'Persembe', 'Cuma', 'Cumartesi', 'Pazar', 'HaftaSonu')),
    kural_yeni_fiyat DECIMAL(10, 2) NOT NULL,
    kural_aciklama NVARCHAR(255),
    FOREIGN KEY (hizmet_id) REFERENCES Hizmetler(hizmet_id) ON DELETE CASCADE,
)

CREATE TABLE Calisan_Hizmetleri
(
    calisan_id INT,
    hizmet_id INT,
    PRIMARY KEY (calisan_id, hizmet_id),
    FOREIGN KEY (calisan_id) REFERENCES Kullanicilar(kullanici_id) ON DELETE CASCADE,
    FOREIGN KEY (hizmet_id) REFERENCES Hizmetler(hizmet_id) ON DELETE CASCADE
);

CREATE TABLE Calisma_Saatleri
(
    calisma_saati_id INT PRIMARY KEY IDENTITY(1,1),
    personel_id INT NOT NULL,
    calisma_gunu NVARCHAR(15) NOT NULL CHECK (calisma_gunu IN ('Pazartesi', 'Sali', 'Carsamba', 'Persembe', 'Cuma', 'Cumartesi', 'Pazar')),
    calisma_baslangic_saati TIME,
    calisma_bitis_saati TIME,
    calisma_musait_mi BIT DEFAULT 1,
    FOREIGN KEY (personel_id) REFERENCES Kullanicilar(kullanici_id) ON DELETE CASCADE
);

CREATE TABLE Randevular
(
    randevu_id INT PRIMARY KEY IDENTITY(1,1),
    musteri_id INT NOT NULL,
    personel_id INT NOT NULL,
    randevu_tarihi_saati DATETIME2 NOT NULL,
    randevu_toplam_tutar DECIMAL(10, 2),
    randevu_durum NVARCHAR(15) NOT NULL CHECK (randevu_durum IN ('Onay Bekliyor', 'Onaylandi', 'Tamamlandi', 'Iptal Edildi', 'Gelmedi')),
    randevu_musteri_notu NVARCHAR(MAX),
    randevu_checkin_zamani DATETIME2 NULL,
    randevu_hizmet_baslama_zamani DATETIME2 NULL,
    randevu_hizmet_bitis_zamani DATETIME2 NULL,
    FOREIGN KEY (musteri_id) REFERENCES Kullanicilar(kullanici_id),
    FOREIGN KEY (personel_id) REFERENCES Kullanicilar(kullanici_id)
);


CREATE TABLE Randevu_Detaylari
(
    randevu_detay_id INT PRIMARY KEY IDENTITY(1,1),
    randevu_id INT NOT NULL,
    hizmet_id INT NOT NULL,
    detay_uygulanan_fiyat DECIMAL(10, 2) NOT NULL,
    detay_hesaplanan_komisyon DECIMAL(10, 2),
    FOREIGN KEY (randevu_id) REFERENCES Randevular(randevu_id) ON DELETE CASCADE,
    FOREIGN KEY (hizmet_id) REFERENCES Hizmetler(hizmet_id)
);

CREATE TABLE Tekrarlanan_Randevular
(
    tekrar_id INT PRIMARY KEY IDENTITY(1,1),
    musteri_id INT NOT NULL,
    personel_id INT NOT NULL,
    hizmet_id INT NOT NULL,
    tekrar_tipi NVARCHAR(10) NOT NULL CHECK (tekrar_tipi IN ('Haftalik', 'Aylik')),
    tekrar_degeri INT NOT NULL,
    tekrar_baslangic_tarihi DATE NOT NULL,
    tekrar_bitis_tarihi DATE,
    FOREIGN KEY (musteri_id) REFERENCES Kullanicilar(kullanici_id) ON DELETE CASCADE,
    FOREIGN KEY (hizmet_id) REFERENCES Hizmetler(hizmet_id),
    FOREIGN KEY (personel_id) REFERENCES Kullanicilar(kullanici_id),

);

CREATE TABLE Odemeler
(
    odeme_id INT PRIMARY KEY IDENTITY(1,1),
    randevu_id INT NOT NULL,
    odeme_tutar DECIMAL(10, 2) NOT NULL,
    odeme_yontemi NVARCHAR(15) NOT NULL CHECK (odeme_yontemi IN ('Kredi Karti', 'Nakit', 'Sadakat Puani','İyzico', 'PayTR')),
    odeme_tarihi DATETIME2 DEFAULT GETDATE(),
    odeme_islem_no NVARCHAR(100),
    FOREIGN KEY (randevu_id) REFERENCES Randevular(randevu_id)
);

CREATE TABLE Kampanyalar
(
    kampanya_id INT PRIMARY KEY IDENTITY(1,1),
    kampanya_ad NVARCHAR(100) NOT NULL,
    kampanya_aciklama NVARCHAR(MAX),
    kampanya_indirim_orani DECIMAL(5, 2),
    kampanya_baslangic_tarihi DATE,
    kampanya_bitis_tarihi DATE
);

CREATE TABLE Musteri_Notlari
(
    not_id INT PRIMARY KEY IDENTITY(1,1),
    musteri_id INT NOT NULL,
    personel_id INT NOT NULL,
    musteri_not_metni NVARCHAR(MAX) NOT NULL,
    musteri_not_olusturma_tarihi DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (musteri_id) REFERENCES Kullanicilar(kullanici_id) ON DELETE CASCADE,
    FOREIGN KEY (personel_id) REFERENCES Kullanicilar(kullanici_id)
);


