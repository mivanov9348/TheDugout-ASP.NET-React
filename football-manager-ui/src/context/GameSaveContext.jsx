import { createContext, useContext, useState } from "react";

const GameSaveContext = createContext();

export function GameSaveProvider({ children }) {
  const [currentGameSave, setCurrentGameSave] = useState(null);

  return (
    <GameSaveContext.Provider value={{ currentGameSave, setCurrentGameSave }}>
      {children}
    </GameSaveContext.Provider>
  );
}

export function useGameSave() {
  return useContext(GameSaveContext);
}
