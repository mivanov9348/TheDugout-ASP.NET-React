// src/context/GameContext.jsx
import React, { createContext, useContext, useCallback, useEffect, useState } from "react";


const GameContext = createContext();

export function GameProvider({ children }) {
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [hasUnplayedMatchesToday, setHasUnplayedMatchesToday] = useState(false);
  const [activeMatch, setActiveMatch] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshGameStatus = useCallback(async () => {
    setIsLoading(true);
    try {
      const res = await fetch("/api/games/current/status", { credentials: "include", method: "POST" });
      if (!res.ok) {
        console.error("refreshGameStatus failed", res.status);
        setIsLoading(false);
        return null;
      }
      const data = await res.json();
      // backend може да върне { gameStatus: { ... } } или директно { ... }
      const status = data.gameStatus ?? data;
      if (status) {
        setCurrentGameSave(status.gameSave ?? null);
        setHasUnplayedMatchesToday(Boolean(status.hasUnplayedMatchesToday));
        setActiveMatch(status.activeMatch ?? null);
      }
      return status;
    } catch (err) {
      console.error("refreshGameStatus error", err);
      return null;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const updateTeamBalance = useCallback((newBalance) => {
  setCurrentGameSave((prev) => {
    if (!prev) return prev;
    return {
      ...prev,
      userTeam: {
        ...prev.userTeam,
        balance: newBalance,
      },
    };
  });
}, []);

  useEffect(() => {
    // в началото опресни
    refreshGameStatus();
  }, [refreshGameStatus]);

  return (
    <GameContext.Provider
      value={{
        currentGameSave,
        setCurrentGameSave,
        hasUnplayedMatchesToday,
        setHasUnplayedMatchesToday,
        activeMatch,
        setActiveMatch,
        isLoading,
        refreshGameStatus,
        updateTeamBalance, 
      }}
    >
      {children}
    </GameContext.Provider>
  );
}

export const useGame = () => useContext(GameContext);
