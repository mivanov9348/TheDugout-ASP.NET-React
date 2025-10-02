import React, { createContext, useContext, useState, useRef } from "react";
import ProcessingOverlay from "../components/ProcessingOverlay";

const ProcessingContext = createContext();

export function ProcessingProvider({ children }) {
  const [logs, setLogs] = useState([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [allowCancel, setAllowCancel] = useState(true);

  const eventSourceRef = useRef(null);
  const abortCtrlRef = useRef(null);

  const startProcessing = (
    msg = "Processing...",
    { allowCancel = true } = {}
  ) => {
    setLogs([msg]); // започваме с нов лог при стартиране
    setIsProcessing(true);
    setAllowCancel(allowCancel);
  };

  const addLog = (msg) =>
    setLogs((prev) => {
      const next = [...prev, msg];
      return next.length > 500 ? next.slice(next.length - 500) : next;
    });

  const stopProcessing = () => {
    if (eventSourceRef.current) {
      try {
        eventSourceRef.current.close();
      } catch (e) {
        console.error("Error closing EventSource", e);
      }
      eventSourceRef.current = null;
    }

    if (abortCtrlRef.current) {
      try {
        abortCtrlRef.current.abort();
      } catch (e) {
        console.error("Error aborting fetch", e);
      }
      abortCtrlRef.current = null;
    }

    setIsProcessing(false);
    setAllowCancel(true);
    // ❌ НЕ чистим логовете – оставяме ги за потребителя
  };

  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

  const runSimulateMatches = async (gameSaveId, { stepDelay = 400 } = {}) => {
    startProcessing("Simulating matches...", { allowCancel: true });

    const ac = new AbortController();
    abortCtrlRef.current = ac;

    try {
      const res = await fetch(`/api/matches/simulate/${gameSaveId}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        signal: ac.signal,
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        addLog(`❌ Error: ${res.status} ${res.statusText} ${txt}`);
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

      for (const m of matches) {
        const homeGoals = m.homeGoals ?? m.HomeGoals ?? "-";
        const awayGoals = m.awayGoals ?? m.AwayGoals ?? "-";
        const isUser =
          m.isUserTeamMatch ?? m.IsUserTeamMatch ? " (Your team)" : "";
        const competition = m.competitionName ?? m.CompetitionName ?? "Match";
        const home = m.home ?? m.Home ?? "Home";
        const away = m.away ?? m.Away ?? "Away";

        const line = `${competition}: ${home} ${homeGoals} - ${awayGoals} ${away}${isUser}`;
        addLog(line);
        await sleep(stepDelay);
      }

      if (data.gameStatus?.gameSave) {
        const gs = data.gameStatus.gameSave;
        const date = gs.Seasons?.[0]?.CurrentDate ?? null;
        if (date) addLog(`Date after sim: ${date}`);
      }

      addLog("✔️ Simulation finished. You can close this window.");
      return data;
    } catch (err) {
      if (err.name === "AbortError") addLog("❌ Simulation aborted.");
      else addLog("❌ Exception: " + (err.message || err));
      throw err;
    } finally {
      await sleep(800);
      abortCtrlRef.current = null;
    }
  };

  return (
    <ProcessingContext.Provider
      value={{
        startProcessing,
        addLog,
        stopProcessing,
        runSimulateMatches,
        isProcessing,
        logs,
        allowCancel,
      }}
    >
      {children}
      <ProcessingOverlay
        logs={logs}
        isProcessing={isProcessing}
        stopProcessing={stopProcessing}
        allowCancel={allowCancel}
      />
    </ProcessingContext.Provider>
  );
}

export function useProcessing() {
  return useContext(ProcessingContext);
}
