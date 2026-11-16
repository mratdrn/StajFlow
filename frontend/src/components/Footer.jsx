import { Link } from "react-router";
import { useAuth } from "./AuthProvider.jsx";

export default function Footer() {
  const { user } = useAuth();
  const isCompany = user?.role === 'Company';
  const isStudent = user?.role === 'Student';

  return <footer className="site-footer" aria-label="Site bilgileri">
    <div className="container footer-grid">
      <section aria-labelledby="platform-hakkinda">
        <Link className="footer-brand" to={isCompany ? '/sirket/ilanlar' : '/staj-ilanlari'}>StajFlow</Link>
        <h2 id="platform-hakkinda">Platform hakkında</h2>
        <p>Öğrencilerin staj fırsatlarına ulaşmasını, şirketlerin başvuruları tek yerden yönetmesini sağlayan platform.</p>
      </section>
      <section aria-labelledby="ogrenciler-icin">
        <h2 id="ogrenciler-icin">Öğrenciler için</h2>
        <p>Fırsatları keşfet, özgeçmişinle başvur ve başvurunun durumunu takip et.</p>
        {!isCompany && <ul>
          <li><Link to="/staj-ilanlari">Staj fırsatlarını keşfet</Link></li>
          {isStudent ? <><li><Link to="/ogrenci/ozgecmisler">CV dosyalarını yönet</Link></li><li><Link to="/ogrenci/basvurular">Başvurularını takip et</Link></li></> :
            <li><Link to="/kayit/ogrenci">Öğrenci hesabı aç</Link></li>}
        </ul>}
      </section>
      <section aria-labelledby="sirketler-icin">
        <h2 id="sirketler-icin">Şirketler için</h2>
        <p>İlan oluştur, aradığın nitelikleri belirle ve adayların başvurularını değerlendir.</p>
        {isCompany ? <ul><li><Link to="/sirket/ilanlar">İlanlarını yönet</Link></li><li><Link to="/sirket/basvurular">Aday başvurularını değerlendir</Link></li><li><Link to="/sirket/gecmis-ilanlar">Önceki ilanlarına ulaş</Link></li></ul> :
          !user && <ul><li><Link to="/kayit/sirket">Şirket hesabı aç</Link></li></ul>}
      </section>
    </div>
    <div className="container footer-bottom">
      <p className="footer-credit">A <span>StajFlow</span> Digital Experience</p>
      <p className="footer-copyright">© {new Date().getFullYear()} StajFlow. All rights reserved.</p>
    </div>
  </footer>;
}
