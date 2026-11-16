import { useEffect } from "react";
import { Link, Navigate, Route, Routes, useLocation, useParams } from "react-router";
import InternshipPostingsPage from "./pages/InternshipPostingsPage.jsx";
import InternshipPostingDetailPage from "./pages/InternshipPostingDetailPage.jsx";
import NotFoundPage from "./pages/NotFoundPage.jsx";
import AuthNavigation from "./components/AuthNavigation.jsx";
import ProtectedRoute from "./components/ProtectedRoute.jsx";
import LoginPage from "./pages/LoginPage.jsx";
import RegistrationPage from "./pages/RegistrationPage.jsx";
import ProfilePage from "./pages/ProfilePage.jsx";
import CompanyPostingsPage from "./pages/CompanyPostingsPage.jsx";
import CompanyPostingEditorPage from "./pages/CompanyPostingEditorPage.jsx";
import ResumesPage from "./pages/ResumesPage.jsx";
import ResumeViewerPage from "./pages/ResumeViewerPage.jsx";
import MyApplicationsPage from "./pages/MyApplicationsPage.jsx";
import ApplicationDetailPage from "./pages/ApplicationDetailPage.jsx";
import IncomingApplicationsPage from "./pages/IncomingApplicationsPage.jsx";
import CompanyApplicationDetailPage from "./pages/CompanyApplicationDetailPage.jsx";
import { useAuth } from "./components/AuthProvider.jsx";
import Footer from "./components/Footer.jsx";

export default function App() {
  const { pathname } = useLocation();
  const { user } = useAuth();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content">İçeriğe geç</a>
      <header className="site-header">
        <div className="container header-inner">
          <Link className="brand" to={user?.role === 'Company' ? '/sirket/ilanlar' : '/staj-ilanlari'} aria-label="StajFlow ana sayfası">
            <span className="brand-mark" aria-hidden="true" />
            <span className="brand-wordmark" aria-hidden="true" />
          </Link>
          <AuthNavigation />
        </div>
      </header>
      <main id="main-content" className="container main-content" tabIndex="-1">
        <Routes>
          <Route path="/" element={<PublicPostingRoute><InternshipPostingsPage /></PublicPostingRoute>} />
          <Route path="/staj-ilanlari" element={<PublicPostingRoute><InternshipPostingsPage /></PublicPostingRoute>} />
          <Route path="/staj-ilanlari/:id" element={<PublicPostingRoute><InternshipPostingDetailPage /></PublicPostingRoute>} />
          <Route path="/internships" element={<Navigate to="/staj-ilanlari" replace />} />
          <Route path="/internships/:id" element={<LegacyPostingRedirect />} />
          <Route path="/login" element={<Navigate to="/giris" replace />} />
          <Route path="/register/student" element={<Navigate to="/kayit/ogrenci" replace />} />
          <Route path="/register/company" element={<Navigate to="/kayit/sirket" replace />} />
          <Route path="/student/profile" element={<Navigate to="/ogrenci/profil" replace />} />
          <Route path="/company/profile" element={<Navigate to="/sirket/profil" replace />} />
          <Route path="/giris" element={<LoginPage />} />
          <Route path="/kayit/ogrenci" element={<RegistrationPage key="student" role="Student" />} />
          <Route path="/kayit/sirket" element={<RegistrationPage key="company" role="Company" />} />
          <Route element={<ProtectedRoute role="Student" />}>
            <Route path="/ogrenci/profil" element={<ProfilePage role="Student" />} />
            <Route path="/ogrenci/ozgecmisler" element={<ResumesPage />} />
            <Route path="/ogrenci/ozgecmisler/:id" element={<ResumeViewerPage />} />
            <Route path="/ogrenci/basvurular" element={<MyApplicationsPage />} />
            <Route path="/ogrenci/basvurular/:id" element={<ApplicationDetailPage />} />
          </Route>
          <Route element={<ProtectedRoute role="Company" />}>
            <Route path="/sirket/profil" element={<ProfilePage role="Company" />} />
            <Route path="/sirket/ilanlar" element={<CompanyPostingsPage />} />
            <Route path="/sirket/gecmis-ilanlar" element={<CompanyPostingsPage archived />} />
            <Route path="/sirket/ilanlar/yeni" element={<CompanyPostingEditorPage key="new" />} />
            <Route path="/sirket/ilanlar/:id/duzenle" element={<CompanyPostingEditorPage key="edit" />} />
            <Route path="/sirket/basvurular" element={<IncomingApplicationsPage />} />
            <Route path="/sirket/basvurular/:id" element={<CompanyApplicationDetailPage />} />
            <Route path="/sirket/basvurular/:id/ozgecmis" element={<ResumeViewerPage companyApplication />} />
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
      <Footer />
    </div>
  );
}

function PublicPostingRoute({ children }) {
  const { user, isLoading } = useAuth();
  if (isLoading) return <p role="status">Oturum denetleniyor…</p>;
  return user?.role === 'Company' ? <Navigate to="/sirket/ilanlar" replace /> : children;
}

function LegacyPostingRedirect() {
  const { id } = useParams();
  return <Navigate to={`/staj-ilanlari/${encodeURIComponent(id)}`} replace />;
}

