import { useState, useEffect } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import LoadGameModal from "./components/LoadGameModal";
import TeamSelectionModal from "./components/TeamSelectionModal"; // 🔹 нов import
import Swal from "sweetalert2";

import Home from "./pages/Home";
import Inbox from "./pages/Inbox";
import Calendar from "./pages/Calendar";
import Squad from "./pages/Squad";
import Tactics from "./pages/Tactics";
import Training from "./pages/Training";
import Transfers from "./pages/Transfers";
import Club from "./pages/Club";
import Finances from "./pages/Finances";
import Competitions from "./pages/Competitions";
import Cup from "./pages/Cup";
import EuropeanCup from "./pages/EuropeanCup";
import League from "./pages/League";
import SearchPlayers from "./pages/SearchPlayers";
import Negotiations from "./pages/Negotiations";
import TransferHistory from "./pages/TransferHistory";
import Fixtures from "./pages/Fixtures";
import PlayerProfile from "./pages/PlayerProfile"; 

// 🔹 ProtectedRoute
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
  const navigate = useNavigate();
  const [pendingSaveId, setPendingSaveId] = useState(null);
  const [showTeamSelection, setShowTeamSelection] = useState(false);
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

  const handleNewGame = async (setStepMessage) => {
    try {
      setStepMessage("Генериране на сейф...");
      const res = await fetch("/api/games/new", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
      });
      const data = await res.json();
      if (!res.ok) throw new Error(data.message || "Грешка при създаване на сейф");

      // ❗ НЕ викаме current/{id} тук!
      setStepMessage("Избор на отбор...");
      setPendingSaveId(data.id);
      setShowTeamSelection(true);

      // не показваме success тук — чак след избора на отбор
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleTeamSelected = (fullSave) => {
    setCurrentGameSave(fullSave);
    setShowTeamSelection(false);
    setPendingSaveId(null);
    Swal.fire("Старт! 🎉", `Избра ${fullSave.userTeamName || "твоя отбор"}. Успех!`, "success");
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

  const handleExitGame = async () => {
    try {
      const res = await fetch("/api/games/exit", {
        method: "POST",
        credentials: "include",
      });
      if (!res.ok) throw new Error("Неуспешно излизане от играта");

      setCurrentGameSave(null);
      navigate("/start");
    } catch (err) {
      console.error(err);
    }
  };

  // ---- Render ----
  if (loading) {
    return <div className="flex items-center justify-center h-screen text-white">Loading...</div>;
  }

  return (
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
              {showTeamSelection && pendingSaveId && (
                <TeamSelectionModal
                  saveId={pendingSaveId}
                  onSelected={handleTeamSelected}
                  onClose={() => {
                    setShowTeamSelection(false);
                    setPendingSaveId(null);
                  }}
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
              <Sidebar onExitGame={handleExitGame} />

              <div className="flex flex-col flex-1">
                <Header currentGameSave={currentGameSave} username={username} />
                <main className="flex-1 overflow-y-auto p-4">
                  <Routes>
                    <Route path="/" element={<Home />} />
                    <Route path="/competitions/*" element={<Competitions />}>
                      <Route index element={<Navigate to="league" replace />} />
                      <Route path="league" element={<League gameSaveId={currentGameSave?.id} />} />
                      <Route path="cup" element={<Cup />} />
                      <Route path="europe" element={<EuropeanCup />} />
                    </Route>

                    <Route path="/inbox" element={<Inbox />} />
                    <Route path="/calendar" element={<Calendar gameSaveId={currentGameSave?.id} />} />
<Route
  path="/squad"
  element={<Squad gameSaveId={currentGameSave?.id} />}
/>
<Route path="/tactics" element={<Tactics gameSaveId={currentGameSave?.id} />} />                    <Route path="/training" element={<Training />} />
                    <Route
                      path="/fixtures"
                      element={
                        <Fixtures
                          gameSaveId={currentGameSave?.id}
                          seasonId={currentGameSave?.seasons?.[0]?.id}
                        />
                      }
                    />
                    <Route path="/transfers" element={<Transfers />}>
                      <Route index element={<Navigate to="search" replace />} />
                      <Route path="search" element={<SearchPlayers gameSaveId={currentGameSave?.id} />} />
                      <Route path="negotiations" element={<Negotiations />} />
                      <Route path="history" element={<TransferHistory />} />
                    </Route>

                    <Route path="/club" element={<Club />} /> 
<Route
  path="/finances"
  element={<Finances gameSaveId={currentGameSave?.id} />}
/>
<Route
                      path="/player/:playerId"
                      element={<PlayerProfile gameSaveId={currentGameSave?.id} />}
                    />
                  </Routes>
                </main>
              </div>
            </div>
          </ProtectedRoute>
        }
      />
    </Routes>
  );
}

export default App;
