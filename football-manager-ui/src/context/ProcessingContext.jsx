// src/contexts/ProcessingContext.jsx
import React, { createContext, useContext, useState } from "react";
import ProcessingOverlay from "../components/ProcessingOverlay";

const ProcessingContext = createContext();

export function ProcessingProvider({ children }) {
  const [logs, setLogs] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);

  const startProcessing = (msg = "Processing...") => {
    setIsProcessing(true);
    setLogs([msg]);
  };

  const addLog = (msg) =>
    setLogs((prev) => {
      // ограничаваме на ~500 реда, ако е нужно
      const next = [...prev, msg];
      return next.length > 500 ? next.slice(next.length - 500) : next;
    });

  const stopProcessing = () => {
    setIsProcessing(false);
    setLogs([]);
  };

  // helper sleep
  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

  // runNextDay - запази настоящата логика (SSE) - без промяна
  const runNextDay = async () => {
    startProcessing("Advancing to next day...");

    const evtSource = new EventSource("/api/game/current/next-day-stream");

    evtSource.onmessage = (e) => {
      try {
        const data = JSON.parse(e.data);

        if (data.type === "progress") {
          addLog(data.message);
        }

        if (data.type === "done") {
          addLog("✅ " + data.message);
          addLog(JSON.stringify(data.extra, null, 2));
          evtSource.close();
          // малко delay преди да скрием overlay за по-приятен UX
          setTimeout(() => stopProcessing(), 700);
        }

        if (data.type === "error") {
          addLog("❌ Error: " + data.message);
          evtSource.close();
          setTimeout(() => stopProcessing(), 800);
        }
      } catch (err) {
        console.error("SSE parse error", err);
      }
    };

    evtSource.onerror = (err) => {
      console.error("SSE error", err);
      addLog("Connection lost.");
      evtSource.close();
      setTimeout(() => stopProcessing(), 800);
    };
  };

  // runSimulateMatches - Variant 1 but step-by-step on client
  const runSimulateMatches = async (gameSaveId, { stepDelay = 400 } = {}) => {
    startProcessing("Simulating matches...");

    try {
      const res = await fetch(`/api/game/simulate/${gameSaveId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        addLog(`❌ Error: ${res.status} ${res.statusText} ${txt}`);
        // оставяме прозореца да се види за кратко и го скриваме
        setTimeout(() => stopProcessing(), 1200);
        return;
      }

      const data = await res.json();
      addLog(`✅ ${data.message}`);

      const matches = data.matches ?? [];
      if (!matches.length) {
        addLog("Няма мачове за симулация (празен списък).");
        setTimeout(() => stopProcessing(), 900);
        return;
      }

      // Показваме всеки мач един по един (step-by-step)
      for (const m of matches) {
        // Форматиране на ред: "League: Home 2 - 1 Away (User match)"
        const homeGoals = m.HomeGoals ?? "-";
        const awayGoals = m.AwayGoals ?? "-";
        const isUser = m.IsUserTeamMatch ? " (Your team)" : "";
        const line = `${m.CompetitionName}: ${m.Home} ${homeGoals} - ${awayGoals} ${m.Away}${isUser}`;
        addLog(line);
        // малко пауза за "стъпков ефект"
        await sleep(stepDelay);
      }

      // Опционално: кратко summary от gameStatus
      try {
        if (data.gameStatus && data.gameStatus.gameSave) {
          const gs = data.gameStatus.gameSave;
          const date = gs.Seasons?.[0]?.CurrentDate ?? null;
          if (date) addLog(`Date after sim: ${date}`);
        }
      } catch (err) {
        // ignore
      }

      addLog("✅ Simulation complete.");
    } catch (err) {
      addLog("❌ Exception: " + (err.message || err));
    } finally {
      // оставяме последния ред видим 0.8s и след това чистим
      await sleep(800);
      stopProcessing();
    }
  };

  return (
    <ProcessingContext.Provider
      value={{ startProcessing, addLog, stopProcessing, runNextDay, runSimulateMatches }}
    >
      {children}
      <ProcessingOverlay logs={logs} isProcessing={isProcessing} stopProcessing={stopProcessing} />
    </ProcessingContext.Provider>
  );
}

export function useProcessing() {
  return useContext(ProcessingContext);
}
