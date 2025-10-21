import React, { createContext, useContext, useCallback, useEffect, useState } from "react";

const GameContext = createContext();

export function GameProvider({ children }) {
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [hasUnplayedMatchesToday, setHasUnplayedMatchesToday] = useState(false);
  const [activeMatch, setActiveMatch] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  const refreshGameStatus = useCallback(async (gameSaveId) => {
    const id = gameSaveId ?? currentGameSave?.id;
    if (!id) {
      console.warn("⚠️ No gameSaveId available for refreshGameStatus");
      return null;
    }

    setIsLoading(true);
    try {
      const res = await fetch(`/api/games/status/${id}`, {
        credentials: "include",
        method: "GET",
      });

      if (!res.ok) {
        console.error("refreshGameStatus failed", res.status);
        setIsLoading(false);
        return null;
      }

      const data = await res.json();
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
  }, [currentGameSave]);

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
    // в началото опресни (ако има save)
    if (currentGameSave?.id) refreshGameStatus(currentGameSave.id);
  }, []);

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
