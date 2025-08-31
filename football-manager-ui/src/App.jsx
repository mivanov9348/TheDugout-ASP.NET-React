// src/App.jsx
import { useState, useEffect } from "react";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
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

// 🔹 ProtectedRoute: пази достъп само ако има сейф
function ProtectedRoute({ isAuthenticated, currentGameSave, children }) {
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  if (!currentGameSave) {
    return <Navigate to="/start" replace />;
  }
  return children;
}

function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [loading, setLoading] = useState(true);
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [userSaves, setUserSaves] = useState([]);
  const [showLoadModal, setShowLoadModal] = useState(false);

  // ---- Auth + сейф check при refresh ----
  useEffect(() => {
    const checkAuthAndSave = async () => {
      try {
        const res = await fetch("/api/auth/me", { credentials: "include" });
        if (!res.ok) {
          setLoading(false);
          return;
        }

        const user = await res.json();
        if (user?.username) {
          setIsAuthenticated(true);
          setUsername(user.username);

          const resSave = await fetch("/api/games/current", { credentials: "include" });
          if (resSave.ok) {
            const fullSave = await resSave.json();
            if (fullSave) setCurrentGameSave(fullSave);
          }
        }
      } catch (err) {
        console.error("Auth/Save check error:", err);
      } finally {
        setLoading(false);
      }
    };

    checkAuthAndSave();
  }, []);

  // ---- функции ----
  const fetchUserSaves = async () => {
    try {
      const res = await fetch("/api/games/saves", { credentials: "include" });
      if (!res.ok) throw new Error("Грешка при зареждане на сейфове");
      const data = await res.json();
      setUserSaves(data);
      setShowLoadModal(true);
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleLoadGame = async (save) => {
    try {
      const res = await fetch(`/api/games/current/${save.id}`, {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) throw new Error("Грешка при зареждане на сейф");
      const fullSave = await res.json();
      setCurrentGameSave(fullSave);
      setShowLoadModal(false);
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleNewGame = async () => {
    try {
      const res = await fetch("/api/games/new", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Грешка при създаване на сейф");

      // 🔹 сетваме новия сейф като current
      const resSet = await fetch(`/api/games/current/${data.id}`, {
        method: "POST",
        credentials: "include",
      });
      if (resSet.ok) {
        const fullSave = await resSet.json();
        setCurrentGameSave(fullSave);
      }

      Swal.fire("Нов сейф 🎉", `Сезон ${new Date(data.seasonStart).getFullYear()} започна.`, "success");
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleDeleteSave = async (saveId) => {
    try {
      const res = await fetch(`/api/games/${saveId}`, { method: "DELETE", credentials: "include" });
      if (!res.ok) {
        const data = await res.json();
        throw new Error(data.message || "Грешка при изтриване на сейф");
      }
      setUserSaves((prev) => prev.filter((s) => s.id !== saveId));
      Swal.fire("Изтрито!", "Сейфът беше успешно изтрит.", "success");
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setUsername("");
    setCurrentGameSave(null);
    fetch("/api/auth/logout", { method: "POST", credentials: "include" });
  };

  // ---- Render ----
  if (loading) {
    return <div className="flex items-center justify-center h-screen text-white">Loading...</div>;
  }

  return (
    <Router>
      <Routes>
        {/* Login */}
        <Route
          path="/login"
          element={
            isAuthenticated ? (
              <Navigate to={currentGameSave ? "/" : "/start"} replace />
            ) : (
              <AuthForm onAuthSuccess={() => setIsAuthenticated(true)} />
            )
          }
        />

        {/* Start screen */}
        <Route
          path="/start"
          element={
            !isAuthenticated ? (
              <Navigate to="/login" replace />
            ) : currentGameSave ? (
              <Navigate to="/" replace />
            ) : (
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
            )
          }
        />

        {/* Protected routes */}
        <Route
          path="/*"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated} currentGameSave={currentGameSave}>
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
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
