import { useState, useEffect } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import LoadGameModal from "./components/LoadGameModal";
import TeamSelectionModal from "./components/TeamSelectionModal";
import { ProcessingProvider } from "./context/ProcessingContext";
import ProcessingOverlay from "./components/ProcessingOverlay";

import Swal from "sweetalert2";

import Home from "./pages/Home";
import Inbox from "./pages/Inbox";
import Calendar from "./pages/Season/Calendar";
import Squad from "./pages/Squad";
import Tactics from "./pages/Tactics";
import Training from "./pages/Training";
import Transfers from "./pages/Transfers";
import Club from "./pages/Club";
import Finances from "./pages/Finances";
import SearchPlayers from "./pages/SearchPlayers";
import Negotiations from "./pages/Negotiations";
import TransferHistory from "./pages/TransferHistory";
import Fixtures from "./pages/Fixtures";
import PlayerProfile from "./pages/PlayerProfile";
import Facilities from "./pages/Facilities";
import Match from "./pages/Match";
import TodayMatches from "./pages/TodayMatches";
import SeasonReview from "./pages/Season/SeasonReview";

import Competitions from "./pages/competitions/Competitions";
import Cup from "./pages/competitions/Cup";
import EuropeanCup from "./pages/competitions/EuropeanCup";
import League from "./pages/competitions/League";

import LeagueStandings from "./pages/competitions/league/Standings";
import LeaguePlayerStats from "./pages/competitions/league/PlayerStats";

import CupPlayerStats from "./pages/competitions/cup/PlayerStats";
import CupKnockouts from "./pages/competitions/cup/Knockouts";

import EuropeGroupStage from "./pages/competitions/europe/GroupStage";
import EuropeKnockouts from "./pages/competitions/europe/Knockouts";
import EuropePlayerStats from "./pages/competitions/europe/PlayerStats";


// 👉 Context
import { GameProvider, useGame } from "./context/GameContext";

// 🔹 ProtectedRoute
function ProtectedRoute({ isAuthenticated, children }) {
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  return children;
}

function AppInner() {
  const navigate = useNavigate();
  const [pendingSaveId, setPendingSaveId] = useState(null);
  const [showTeamSelection, setShowTeamSelection] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [username, setUsername] = useState("");
  const [loading, setLoading] = useState(true);
  const [userSaves, setUserSaves] = useState([]);
  const [showLoadModal, setShowLoadModal] = useState(false);

  const { currentGameSave, setCurrentGameSave } = useGame();

  // ---- Auth + сейф check при refresh ----
  useEffect(() => {
    const checkAuthAndSave = async () => {
      try {
        setLoading(true);
        const res = await fetch("/api/auth/me", { credentials: "include" });

        if (!res.ok) {
          setIsAuthenticated(false);
          setLoading(false);
          return;
        }

        const user = await res.json();
        if (user?.username) {
          setIsAuthenticated(true);
          setUsername(user.username);

          // Зареди текущия save
          const resSave = await fetch("/api/games/current", {
            credentials: "include",
          });

          if (resSave.ok) {
            const fullSave = await resSave.json();
            setCurrentGameSave(fullSave);
          } else {
            setCurrentGameSave(null);
          }
        }
      } catch (err) {
        console.error("Auth/Save check error:", err);
        setIsAuthenticated(false);
        setCurrentGameSave(null);
      } finally {
        setLoading(false);
      }
    };

    checkAuthAndSave();
  }, [setCurrentGameSave]);

  // ---- Функция за успешен login ----
  const handleAuthSuccess = async () => {
    try {
      setIsAuthenticated(true);

      // Зареди user информация
      const resUser = await fetch("/api/auth/me", { credentials: "include" });
      if (resUser.ok) {
        const user = await resUser.json();
        setUsername(user.username);
      }

      // Провери дали има активна игра
      const resSave = await fetch("/api/games/current", {
        credentials: "include",
      });

      if (resSave.ok) {
        const fullSave = await resSave.json();
        if (fullSave) {
          setCurrentGameSave(fullSave);
          navigate("/"); // Ако има активна игра, отиваме в играта
        } else {
          navigate("/start"); // Ако няма, отиваме в start screen
        }
      } else {
        navigate("/start");
      }
    } catch (err) {
      console.error("Error after auth success:", err);
      navigate("/start");
    }
  };

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
      if (!res.ok)
        throw new Error(data.message || "Грешка при създаване на сейф");

      setStepMessage("Избор на отбор...");
      setPendingSaveId(data.id);
      setShowTeamSelection(true);
    } catch (err) {
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleTeamSelected = (fullSave) => {
    setCurrentGameSave(fullSave);
    setShowTeamSelection(false);
    setPendingSaveId(null);
    Swal.fire(
      "Старт! 🎉",
      `Избра ${fullSave.userTeamName || "твоя отбор"}. Успех!`,
      "success"
    );
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
      Swal.fire("Грешка!", err.message, "error");
    }
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setUsername("");
    setCurrentGameSave(null);
    fetch("/api/auth/logout", { method: "POST", credentials: "include" });
    navigate("/login");
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
    return (
      <div className="flex items-center justify-center h-screen text-white">
        Loading...
      </div>
    );
  }

  return (
    <>
      <Routes>
        {/* Login */}
        <Route
          path="/login"
          element={
            isAuthenticated ? (
              <Navigate to={currentGameSave ? "/" : "/start"} replace />
            ) : (
              <AuthForm onAuthSuccess={handleAuthSuccess} />
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

        {/* Protected routes - САМО АКО ИМА АКТИВНА ИГРА */}
        <Route
          path="/*"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              {!currentGameSave ? (
                <Navigate to="/start" replace />
              ) : (
                <div className="flex h-screen bg-slate-100">
                  <Sidebar onExitGame={handleExitGame} />

                  <div className="flex flex-col flex-1">
                    <Header username={username} />
                    <main className="flex-1 overflow-y-auto p-4">
                      <Routes>
                        <Route
                          path="/"
                          element={<Home gameSaveId={currentGameSave?.id} />}
                        />

                        <Route
                          path="/competitions/*"
                          element={<Competitions gameSaveId={currentGameSave?.id} />}
                        >
                          {/* Default redirect */}
                          <Route index element={<Navigate to="league" replace />} />

                          {/* --- League --- */}
                          <Route
                            path="league/*"
                            element={
                              <League
                                gameSaveId={currentGameSave?.id}
                                seasonId={currentGameSave?.seasons?.[0]?.id}
                              />
                            }
                          >
                            <Route index element={<Navigate to="standings" replace />} />
                            <Route path="standings" element={<LeagueStandings />} />
                            <Route path="player-stats" element={<LeaguePlayerStats />} />
                          </Route>

                          {/* --- Cup --- */}
                          <Route
                            path="cup/*"
                            element={
                              <Cup
                                gameSaveId={currentGameSave?.id}
                                seasonId={currentGameSave?.seasons?.[0]?.id}
                              />
                            }
                          >
                            <Route index element={<Navigate to="knockouts" replace />} />
                            <Route path="knockouts" element={<CupKnockouts />} />
                            <Route path="player-stats" element={<CupPlayerStats />} />
                          </Route>

                          {/* --- European Cup --- */}
                          <Route
                            path="europe/*"
                            element={
                              <EuropeanCup
                                gameSaveId={currentGameSave?.id}
                                seasonId={currentGameSave?.seasons?.[0]?.id}
                              />
                            }
                          >
                            <Route index element={<Navigate to="standings" replace />} />
                            <Route path="groupstage" element={<EuropeGroupStage />} />
                            <Route path="knockouts" element={<EuropeKnockouts />} />
                            <Route path="player-stats" element={<EuropePlayerStats />} />
                          </Route>
                        </Route>

                        {/* Other main routes */}
                        <Route
                          path="/inbox"
                          element={<Inbox gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/calendar"
                          element={<Calendar gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/squad"
                          element={<Squad gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/tactics"
                          element={
                            <Tactics
                              gameSaveId={currentGameSave?.id}
                              teamId={currentGameSave?.userTeamId}
                            />
                          }
                        />
                        <Route
                          path="/training"
                          element={
                            <Training
                              gameSaveId={currentGameSave?.id}
                              teamId={currentGameSave?.userTeamId}
                            />
                          }
                        />
                        <Route
                          path="/fixtures"
                          element={
                            <Fixtures
                              gameSaveId={currentGameSave?.id}
                              seasonId={currentGameSave?.seasons?.[0]?.id}
                            />
                          }
                        />
                        <Route
                          path="/transfers/*"
                          element={<Transfers gameSaveId={currentGameSave?.id} />}
                        >
                          <Route index element={<Navigate to="search" replace />} />
                          <Route
                            path="search"
                            element={<SearchPlayers gameSaveId={currentGameSave?.id} />}
                          />
                          <Route
                            path="negotiations"
                            element={<Negotiations gameSaveId={currentGameSave?.id} />}
                          />
                          <Route
                            path="history"
                            element={<TransferHistory gameSaveId={currentGameSave?.id} />}
                          />
                        </Route>

                        <Route
                          path="/facilities"
                          element={
                            <Facilities
                              gameSaveId={currentGameSave?.id}
                              teamId={currentGameSave?.userTeamId}
                            />
                          }
                        />
                        <Route
                          path="/club"
                          element={<Club gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/finances"
                          element={<Finances gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/player/:playerId"
                          element={<PlayerProfile gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/match"
                          element={<Match gameSaveId={currentGameSave?.id} />}
                        />
                        <Route
                          path="/today-matches/:gameSaveId"
                          element={<TodayMatches />}
                        />
                      
                        <Route
                          path="/season-review"
                          element={<SeasonReview gameSaveId={currentGameSave?.id} />}
                        />
                        <Route path="/match/:matchId" element={<Match />} />
                        <Route path="*" element={<div>404 Not Found</div>} />
                      </Routes>
                    </main>
                  </div>
                </div>
              )}
            </ProtectedRoute>
          }
        />
      </Routes>
      <ProcessingOverlay />
    </>
  );
}

export default function App() {
  return (
    <GameProvider>
      <ProcessingProvider>
        <AppInner />
      </ProcessingProvider>
    </GameProvider>
  );
}