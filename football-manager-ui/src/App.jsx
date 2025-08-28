// src/App.jsx
import { useState, useEffect } from "react";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import MainContent from "./components/MainContent";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import LoadGameModal from "./components/LoadGameModal";
import Swal from "sweetalert2";

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [inMenu, setInMenu] = useState(true);
  const [activePage, setActivePage] = useState("Home");
  const [loading, setLoading] = useState(true);
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [userSaves, setUserSaves] = useState([]);
  const [showLoadModal, setShowLoadModal] = useState(false);

  // 🔹 Зареждане на auth + state от localStorage
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const res = await fetch("/api/auth/me", { credentials: "include" });
        if (res.ok) {
          const data = await res.json();
          if (data?.username) {
            setIsAuthenticated(true);
            setUsername(data.username);

            // ако имаме сейф в localStorage → възстановяваме
            const savedGame = localStorage.getItem("currentGameSave");
            const savedPage = localStorage.getItem("activePage");
            if (savedGame) {
              setCurrentGameSave(JSON.parse(savedGame));
              setInMenu(false);
            }
            if (savedPage) {
              setActivePage(savedPage);
            }
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

  // 🔹 Записваме currentGameSave в localStorage
  useEffect(() => {
    if (currentGameSave) {
      localStorage.setItem("currentGameSave", JSON.stringify(currentGameSave));
    } else {
      localStorage.removeItem("currentGameSave");
    }
  }, [currentGameSave]);

  // 🔹 Записваме активната страница
  useEffect(() => {
    if (activePage) {
      localStorage.setItem("activePage", activePage);
    }
  }, [activePage]);

  const fetchUserSaves = async () => {
    try {
      const res = await fetch("/api/games/saves", { credentials: "include" });
      if (!res.ok) throw new Error("Грешка при зареждане на сейфове");
      const data = await res.json();
      setUserSaves(data);
      setShowLoadModal(true);
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

  const handleLoadGame = (save) => {
    setCurrentGameSave(save);
    setInMenu(false);
    setShowLoadModal(false);
  };

  const handleNewGame = async () => {
    try {
      const res = await fetch("/api/games/new", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ teamTemplateId: 0 }),
      });

      const data = await res.json();

      if (!res.ok) {
        throw new Error(data.message || "Грешка при създаване на сейф");
      }

      setCurrentGameSave(data);
      setInMenu(false);

      Swal.fire({
        title: "Нов сейф създаден! 🎉",
        text: `Сезон ${new Date(data.seasonStart).getFullYear()} започна успешно.`,
        icon: "success",
        confirmButtonText: "OK",
      });
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleDeleteSave = async (saveId) => {
    try {
      const res = await fetch(`/api/games/${saveId}`, {
        method: "DELETE",
        credentials: "include",
      });

      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Грешка при изтриване на сейф");
      }

      setUserSaves((prev) => prev.filter((s) => s.id !== saveId));

      Swal.fire("Изтрито!", "Сейфът беше успешно изтрит.", "success");
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setUsername("");
    setInMenu(true);
    setCurrentGameSave(null);
    localStorage.clear(); // 🔹 чистим state при logout
    fetch("/api/auth/logout", { method: "POST", credentials: "include" });
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
      <>
        <StartScreen
          username={username}
          onNewGame={handleNewGame}
          onLoadGame={fetchUserSaves}
          onLogout={handleLogout}
        />
        {showLoadModal && (
          <LoadGameModal
            saves={userSaves}
            onClose={() => setShowLoadModal(false)}
            onSelectSave={handleLoadGame}
            onDeleteSave={handleDeleteSave}
          />
        )}
      </>
    );
  }

  return (
    <div className="flex h-screen bg-slate-100">
      <Sidebar activePage={activePage} setActivePage={setActivePage} />
      <div className="flex flex-col flex-1">
        <Header />
        <MainContent activePage={activePage} currentGameSave={currentGameSave} />
      </div>
    </div>
  );
}

export default App;
