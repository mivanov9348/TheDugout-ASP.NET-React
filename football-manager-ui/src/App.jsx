// App.jsx
import { useState, useEffect } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import Swal from "sweetalert2";

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [inMenu, setInMenu] = useState(true);
  const [activePage, setActivePage] = useState("Home");
  const [loading, setLoading] = useState(true);
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

  const handleNewGame = async () => {
  try {
    const res = await fetch("http://localhost:7117/api/games/new", {
  method: "POST",
  headers: {
    "Content-Type": "application/json",
  },
  credentials: "include",
  body: JSON.stringify({
    teamTemplateId: 0 
  }),
});

    if (!res.ok) throw new Error("Грешка при създаване на сейф");

    const data = await res.json();
    setCurrentGameSave(data);

    Swal.fire({
      title: "Нов сейф създаден! 🎉",
      text: `Сезон ${new Date(data.seasonStart).getFullYear()} започна успешно.`,
      icon: "success",
      confirmButtonText: "OK",
    });

  } catch (err) {
    console.error(err);
    Swal.fire({
      title: "Грешка!",
      text: err.message,
      icon: "error",
      confirmButtonText: "OK",
    });
  }
};


  if (loading) {
    return (
      <div className="flex items-center justify-center h-screen text-white">
        Loading...
      </div>
    );
  }

  if (!isAuthenticated) {
    return <AuthForm onAuthSuccess={() => setIsAuthenticated(true)} />;
  }

  if (inMenu) {
    return (
      <StartScreen
        username={username}
        onNewGame={handleNewGame}
        onLoadGame={() => alert("Load Game още не е готово")}
        onLogout={() => {
          setIsAuthenticated(false);
          setUsername("");
          setInMenu(true);
          setCurrentGameSave(null);
          fetch("/api/auth/logout", { method: "POST", credentials: "include" });
        }}
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
