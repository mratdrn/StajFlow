import { useEffect } from "react";
import StatePanel from "../components/StatePanel.jsx";

export default function NotFoundPage() {
  useEffect(() => { document.title = 'Sayfa bulunamadı | StajFlow'; }, []);
  return <><h1 className="page-title">Sayfa bulunamadı</h1><StatePanel title="İlanlara geri dönelim" backLink>Aradığın sayfa bulunamadı. Güncel staj ilanlarına göz atabilirsin.</StatePanel></>;
}

