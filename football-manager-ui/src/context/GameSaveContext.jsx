import { createContext, useContext, useState, useEffect } from "react";

const GameSaveContext = createContext();

export function GameSaveProvider({ children }) {
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [isLoading, setIsLoading] = useState(true);

  // 👇 ВЪЗСТАНОВЯВАНЕ ОТ localStorage ПРИ ЗАРЕЖДАНЕ
  useEffect(() => {
    const savedGameSave = localStorage.getItem('currentGameSave');
    if (savedGameSave) {
      try {
        setCurrentGameSave(JSON.parse(savedGameSave));
      } catch (err) {
        console.error("Failed to parse saved game:", err);
        localStorage.removeItem('currentGameSave');
      }
    }
    setIsLoading(false);
  }, []);

  // 👇 АВТОMATIC ЗАПАЗВАНЕ В localStorage ПРИ ПРОМЯНА
  useEffect(() => {
    if (currentGameSave) {
      localStorage.setItem('currentGameSave', JSON.stringify(currentGameSave));
    } else {
      localStorage.removeItem('currentGameSave');
    }
  }, [currentGameSave]);

  // 👇 ПО-УДОБНА ФУНКЦИЯ ЗА ОБНОВЯВАНЕ
  const updateGameSave = (newData) => {
    setCurrentGameSave(prev => {
      if (!prev) return newData;
      return { ...prev, ...newData };
    });
  };

  // 👇 ФУНКЦИЯ ЗА ИЗЧИСТВАНЕ
  const clearGameSave = () => {
    setCurrentGameSave(null);
    localStorage.removeItem('currentGameSave');
  };

  const value = {
    currentGameSave,
    setCurrentGameSave,
    updateGameSave,
    clearGameSave,
    isLoading
  };

  return (
    <GameSaveContext.Provider value={value}>
      {children}
    </GameSaveContext.Provider>
  );
}

export function useGameSave() {
  const context = useContext(GameSaveContext);
  if (!context) {
    throw new Error("useGameSave must be used within a GameSaveProvider");
  }
  return context;
}