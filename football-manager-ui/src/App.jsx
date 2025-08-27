import { useState, useEffect } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [inMenu, setInMenu] = useState(true);
  const [activePage, setActivePage] = useState("Home");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
  const checkAuth = async () => {
    try {
      const res = await fetch("/api/auth/me", { credentials: "include" });
      console.log(res);
      if (res.ok) {
        const data = await res.json();
        console.log(data);
        if (data?.username) { 
          console.log(data);
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
    return <div className="flex items-center justify-center h-screen text-white">Loading...</div>;
  }

  if (!isAuthenticated) {
    return <AuthForm onAuthSuccess={() => setIsAuthenticated(true)} />;
  }

  if (inMenu) {
    return (
      <StartScreen
        username={username}
        onNewGame={() => setInMenu(false)}
        onLoadGame={() => alert("Load Game още не е готово")}
      />
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
