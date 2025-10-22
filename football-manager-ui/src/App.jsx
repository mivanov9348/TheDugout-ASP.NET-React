import { useState, useEffect } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import Swal from "sweetalert2";

import Header from "./components/Header";
import Sidebar from "./components/Sidebar";
import AuthForm from "./components/AuthForm";
import StartScreen from "./components/StartScreen";
import LoadGameModal from "./components/LoadGameModal";
import TeamSelectionModal from "./components/TeamSelectionModal";
import ProcessingOverlay from "./components/ProcessingOverlay";
import SeasonOverview from "./pages/season/SeasonOverview";

import { ProcessingProvider } from "./context/ProcessingContext";
import { GameProvider, useGame } from "./context/GameContext";

import { AppRoutes } from "./routes/AppRoutes";
import ProtectedRoute from "./routes/ProtectedRoute";

function AppInner() {
  const navigate = useNavigate();
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [loading, setLoading] = useState(true);
  const [username, setUsername] = useState("");
  const [showLoadModal, setShowLoadModal] = useState(false);
  const [userSaves, setUserSaves] = useState([]);
  const [showTeamSelection, setShowTeamSelection] = useState(false);
  const [pendingSaveId, setPendingSaveId] = useState(null);

  const { currentGameSave, setCurrentGameSave } = useGame();

  useEffect(() => {
    const checkAuthAndSave = async () => {
      try {
        setLoading(true);
        const res = await fetch("/api/auth/me", { credentials: "include" });
        if (!res.ok) return setIsAuthenticated(false);

        const user = await res.json();
        if (user?.username) {
          setIsAuthenticated(true);
          setUsername(user.username);

          const resSave = await fetch("/api/games/current", { credentials: "include" });
          if (resSave.ok) {
            setCurrentGameSave(await resSave.json());
          }
        }
      } catch (err) {
        console.error("Auth check error:", err);
      } finally {
        setLoading(false);
      }
    };

    checkAuthAndSave();
  }, [setCurrentGameSave]);

  if (loading)
    return <div className="flex items-center justify-center h-screen text-white">Loading...</div>;

  return (
    <>
      <Routes>
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

        <Route
          path="/start"
          element={
            !isAuthenticated ? (
              <Navigate to="/login" replace />
            ) : currentGameSave ? (
              <Navigate to="/" replace />
            ) : (
              <StartScreen
                username={username}
                onNewGame={() => console.log("new game")}
                onLoadGame={() => setShowLoadModal(true)}
                onLogout={() => setIsAuthenticated(false)}
              />
            )
          }
        />
        <Route
          path="/season/:seasonId/overview"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              <SeasonOverview />
            </ProtectedRoute>
          }
        />

        <Route
          path="/*"
          element={
            <ProtectedRoute isAuthenticated={isAuthenticated}>
              {!currentGameSave ? (
                <Navigate to="/start" replace />
              ) : (
                <div className="flex h-screen bg-slate-100">
                  <Sidebar />
                  <div className="flex flex-col flex-1">
                    <Header username={username} />
                    <main className="flex-1 overflow-y-auto p-4">
                      <Routes>
                        {AppRoutes(
                          currentGameSave?.id,
                          currentGameSave?.userTeamId,
                          currentGameSave?.seasons?.[0]?.id
                        )}
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
