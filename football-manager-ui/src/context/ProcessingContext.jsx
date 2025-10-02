// src/contexts/ProcessingContext.jsx
import React, { createContext, useContext, useState, useRef } from "react";
import ProcessingOverlay from "../components/ProcessingOverlay";

const ProcessingContext = createContext();

export function ProcessingProvider({ children }) {
  const [logs, setLogs] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [allowCancel, setAllowCancel] = useState(true);

  // refs so we can close/abort from stopProcessing
  const eventSourceRef = useRef(null);
  const abortCtrlRef = useRef(null);

  const startProcessing = (msg = "Processing...", { allowCancel = true } = {}) => {
    setLogs([msg]);
    setIsProcessing(true);
    setAllowCancel(allowCancel);
  };

  const addLog = (msg) =>
    setLogs((prev) => {
      const next = [...prev, msg];
      return next.length > 500 ? next.slice(next.length - 500) : next;
    });

  const stopProcessing = () => {
    // close SSE if present
    if (eventSourceRef.current) {
      try {
        eventSourceRef.current.close();
      } catch (e) {}
      eventSourceRef.current = null;
    }

    // abort ongoing fetch if present
    if (abortCtrlRef.current) {
      try {
        abortCtrlRef.current.abort();
      } catch (e) {}
      abortCtrlRef.current = null;
    }

    setIsProcessing(false);
    setAllowCancel(true);
    setLogs([]);
  };

  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

  // runNextDay left mostly as SSE example (we keep ability to cancel)
  const runNextDay = async () => {
    startProcessing("Advancing to next day...", { allowCancel: true });

    const evtSource = new EventSource("/api/game/current/next-day-stream");
    eventSourceRef.current = evtSource;

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
          eventSourceRef.current = null;
          setTimeout(() => stopProcessing(), 700);
        }

        if (data.type === "error") {
          addLog("❌ Error: " + data.message);
          evtSource.close();
          eventSourceRef.current = null;
          setTimeout(() => stopProcessing(), 800);
        }
      } catch (err) {
        console.error("SSE parse error", err);
      }
    };

    evtSource.onerror = (err) => {
      console.error("SSE error", err);
      addLog("Connection lost.");
      try {
        evtSource.close();
      } catch (e) {}
      eventSourceRef.current = null;
      setTimeout(() => stopProcessing(), 800);
    };
  };

  // runSimulateMatches — BLOCKING overlay (allowCancel=false) and RETURNS data
  const runSimulateMatches = async (gameSaveId, { stepDelay = 400 } = {}) => {
    startProcessing("Simulating matches...", { allowCancel: false });

    // create abort controller in case we ever want to abort
    const ac = new AbortController();
    abortCtrlRef.current = ac;

    try {
      const res = await fetch(`/api/matches/simulate/${gameSaveId}`, {
      const res = await fetch(`/api/matches/simulate/${gameSaveId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        signal: ac.signal,
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        addLog(`❌ Error: ${res.status} ${res.statusText} ${txt}`);
        // keep overlay visible briefly so user can read
        await sleep(1200);
        return null;
      }

      const data = await res.json();
      addLog(`✅ ${data.message}`);

      const matches = data.matches ?? [];
      if (!matches.length) {
        addLog("Няма мачове за симулация (празен списък).");
        await sleep(900);
        return data;
      }

      // step-by-step log every match (visual effect)
      for (const m of matches) {
        // Форматиране на ред: "League: Home 2 - 1 Away (User match)"
        const homeGoals = m.HomeGoals ?? "-";
        const awayGoals = m.AwayGoals ?? "-";
        const isUser = m.IsUserTeamMatch ? " (Your team)" : "";
        const line = `${m.CompetitionName}: ${m.Home} ${homeGoals} - ${awayGoals} ${m.Away}${isUser}`;
        addLog(line);
        await sleep(stepDelay);
      }

      // optional summary from gameStatus
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
      return data;
    } catch (err) {
      if (err.name === "AbortError") addLog("❌ Simulation aborted.");
      else addLog("❌ Exception: " + (err.message || err));
      throw err;
    } finally {
      // оставяме последния ред видим 0.8s и след това чистим
      await sleep(800);
      abortCtrlRef.current = null;
      stopProcessing();
      // рефреш на страницата след като се скрие overlay
      setTimeout(() => {
        window.location.reload();
      }, 200);
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
