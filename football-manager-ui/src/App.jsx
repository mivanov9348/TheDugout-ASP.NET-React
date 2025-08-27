import { useState, useEffect } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import TeamSelectionModal from "./components/TeamSelectionModal";
function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [inMenu, setInMenu] = useState(true);
  const [activePage, setActivePage] = useState("Home");
  const [loading, setLoading] = useState(true);
  // NEW
  const [showTeamModal, setShowTeamModal] = useState(false);
  const [currentGameSave, setCurrentGameSave] = useState(null);
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const res = await fetch("/api/auth/me", { credentials: "include" });
        if (res.ok) {
          const data = await res.json();
          if (data?.username) {
            setIsAuthenticated(true);
            setUsername(data.username);
          }
        }
      } catch (err) {
        console.error("Auth check error:", err);
      } finally {
        setLoading(false);
      }
    };
    checkAuth();
  }, []);
  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen textwhite">
        Loading...
      </div>
    );
  }
  if (!isAuthenticated) {
    return <AuthForm onAuthSuccess={() => setIsAuthenticated(true)} />;
  }
  if (inMenu) {
    return (
      <>
        <StartScreen
          username={username}
          onNewGame={() => setShowTeamModal(true)}
          onLoadGame={() => alert("Load Game още не е готово")}
          onLogout={() => {
    setIsAuthenticated(false);
    setUsername("");
    setInMenu(true);
    setCurrentGameSave(null);

    fetch("/api/auth/logout", { method: "POST", credentials: "include" });
  }}
        />
        <TeamSelectionModal
          open={showTeamModal}
          onClose={() => setShowTeamModal(false)}
          onSuccess={(saveResp) => {
            // saveResp = { gameSaveId, userTeamId, userTeamName }
            setCurrentGameSave(saveResp);
            setShowTeamModal(false);
            setInMenu(false); // влизаме в играта
          }}
        />
      </>
    );
  }
  return (
    <div className="flex h-screen bg-slate-100">
      <Sidebar activePage={activePage} setActivePage={setActivePage} />
      <div className="flex flex-col flex-1">
        <Header />
        <MainContent activePage={activePage} />
      </div>
    </div>
  );
}
export default App;
