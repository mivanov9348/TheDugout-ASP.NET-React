// src/App.jsx
import { useState, useEffect } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import LoadGameModal from "./components/LoadGameModal";
import Swal from "sweetalert2";

// Pages
import Home from "./pages/Home";
import Inbox from "./pages/Inbox";
import Calendar from "./pages/Calendar";
import Squad from "./pages/Squad";
import Tactics from "./pages/Tactics";
import Training from "./pages/Training";
import Schedule from "./pages/Schedule";
import League from "./pages/League";
import Transfers from "./pages/Transfers";
import Club from "./pages/Club";
import Finances from "./pages/Finances";
import Players from "./pages/Players";

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [loading, setLoading] = useState(true);
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [userSaves, setUserSaves] = useState([]);
  const [showLoadModal, setShowLoadModal] = useState(false);

  // 🔹 Auth check при refresh
  useEffect(() => {
    const checkAuth = async () => {
      try {
        const res = await fetch("/api/auth/me", { credentials: "include" });
        if (res.ok) {
          const data = await res.json();
          if (data?.username) {
            setIsAuthenticated(true);
            setUsername(data.username);

            // 🔹 Load game from saved id
            const savedGameId = localStorage.getItem("currentGameSave");
            if (savedGameId) {
              try {
                const resSave = await fetch(`/api/games/${savedGameId}`, { credentials: "include" });
                if (resSave.ok) {
                  const fullSave = await resSave.json();
                  setCurrentGameSave(fullSave);
                } else {
                  localStorage.removeItem("currentGameSave"); // сейфът е изтрит
                }
              } catch (err) {
                console.error("Грешка при зареждане на сейф от бекенда:", err);
              }
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

  // 🔹 Persist only save id
  useEffect(() => {
    if (currentGameSave?.id) {
      localStorage.setItem("currentGameSave", currentGameSave.id);
    } else {
      localStorage.removeItem("currentGameSave");
    }
  }, [currentGameSave]);

  const fetchUserSaves = async () => {
    try {
      const res = await fetch("/api/games/saves", { credentials: "include" });
      if (!res.ok) throw new Error("Грешка при зареждане на сейфове");
      const data = await res.json();
      setUserSaves(data);
      setShowLoadModal(true);
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleLoadGame = async (save) => {
    try {
      const res = await fetch(`/api/games/${save.id}`, { credentials: "include" });
      if (!res.ok) throw new Error("Грешка при зареждане на сейф");

      const fullSave = await res.json();
      setCurrentGameSave(fullSave);

      // ✅ Save only id
      localStorage.setItem("currentGameSave", save.id);

      setShowLoadModal(false);
    } catch (err) {
      console.error(err);
      Swal.fire("Грешка!", err.message, "error");
    }
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
      if (!res.ok) throw new Error(data.message || "Грешка при създаване на сейф");

      setCurrentGameSave(data);

      // ✅ Save only id
      localStorage.setItem("currentGameSave", data.id);

      Swal.fire(
        "Нов сейф 🎉",
        `Сезон ${new Date(data.seasonStart).getFullYear()} започна.`,
        "success"
      );
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
    setCurrentGameSave(null);
    localStorage.clear();
    fetch("/api/auth/logout", { method: "POST", credentials: "include" });
  };

  // ---- Render logic ----
  if (loading) {
    return <div className="flex items-center justify-center h-screen text-white">Loading...</div>;
  }

  if (!isAuthenticated) {
    return <AuthForm onAuthSuccess={() => setIsAuthenticated(true)} />;
  }

  if (!currentGameSave) {
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

  // Ако има сейф → директно в играта
  return (
    <Router>
      <div className="flex h-screen bg-slate-100">
        <Sidebar />
        <div className="flex flex-col flex-1">
          <Header currentGameSave={currentGameSave} username={username} />
          <main className="flex-1 overflow-y-auto p-4">
            <Routes>
              <Route path="/" element={<Home />} />
              <Route path="/league" element={<League gameSaveId={currentGameSave?.id} />} />
              <Route path="/inbox" element={<Inbox />} />
              <Route path="/calendar" element={<Calendar />} />
              <Route path="/squad" element={<Squad />} />
              <Route path="/tactics" element={<Tactics />} />
              <Route path="/training" element={<Training />} />
              <Route path="/schedule" element={<Schedule />} />
              <Route path="/transfers" element={<Transfers />} />
              <Route path="/club" element={<Club />} />
              <Route path="/finances" element={<Finances />} />
<Route path="/players" element={<Players gameSaveId={currentGameSave?.id} />} />
            </Routes>
          </main>
        </div>
      </div>
    </Router>
  );
}

export default App;
