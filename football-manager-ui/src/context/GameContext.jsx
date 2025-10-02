// src/context/GameContext.jsx
import { createContext, useContext, useState, useEffect } from "react";
import { HubConnectionBuilder } from "@microsoft/signalr";

const GameContext = createContext();

export function GameProvider({ children }) {
  const [currentGameSave, setCurrentGameSave] = useState(null);
  const [connection, setConnection] = useState(null);

  // SignalR връзка
  useEffect(() => {
    const newConnection = new HubConnectionBuilder()
      .withUrl("/gameHub", {
        withCredentials: true, // Изпраща бисквитки за аутентикация
        // Ако използваш JWT токен, добави:
        // accessTokenFactory: () => localStorage.getItem("token"),
      })
      .withAutomaticReconnect()
      .build();

    setConnection(newConnection);

    return () => {
      if (newConnection) {
        newConnection.stop();
      }
    };
  }, []);

  // Стартиране на SignalR връзка
  useEffect(() => {
    if (connection) {
      const startConnection = async () => {
        try {
          await connection.start();
          console.log("SignalR свързан успешно");
        } catch (err) {
          console.error("SignalR грешка:", err);
          if (err.message.includes("401")) {
            window.location.href = "/login";
          }
        }
      };
      startConnection();
    }
  }, [connection]);

  return (
    <GameContext.Provider value={{ currentGameSave, setCurrentGameSave, connection }}>
      {children}
    </GameContext.Provider>
  );
}

export function useGame() {
  const context = useContext(GameContext);
  if (!context) {
    throw new Error("useGame трябва да се използва в GameProvider");
  }
  return context;
}