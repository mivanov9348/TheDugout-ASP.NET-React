import React, { createContext, useContext, useState, useRef } from "react";
import { flushSync } from "react-dom";
import ProcessingOverlay from "../components/ProcessingOverlay";
import Swal from "sweetalert2";

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
    setLogs([msg]);
    setIsProcessing(true);
    setAllowCancel(allowCancel);
  };

  const addLog = (msg) => {
    flushSync(() => {
      setLogs((prev) => {
        const next = [...prev, msg];
        return next.length > 500 ? next.slice(next.length - 500) : next;
      });
    });
  };

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
  };

  const sleep = (ms) => new Promise((r) => setTimeout(r, ms));

  const runSimulateMatches = async (gameSaveId) => {
    startProcessing("Simulating matches...", { allowCancel: true });

    const es = new EventSource(`/api/matches/simulate-stream/${gameSaveId}`);
    eventSourceRef.current = es;

    let firstMessageHandled = false;

    return new Promise((resolve) => {
      es.onmessage = (e) => {
        if (!e.data) return;
        try {
          const data = JSON.parse(e.data);

          if (data.error) {
            stopProcessing();
            es.close();

            // 🧠 Показваме SweetAlert
            Swal.fire({
  icon: "error",
  title: "No tactic found ⚠️",
  html: `
    <p>Your team has a scheduled match today but no tactic is saved.</p>
    <p class="mt-2 text-sm text-gray-500">Please create or load a tactic before simulating matches.</p>
  `,
  confirmButtonColor: "#1d4ed8",
  confirmButtonText: "Go to Tactics",
}).then((r) => {
  if (r.isConfirmed) window.location.href = "http://localhost:5173/tactics";
});


            addLog(`❌ ${data.error}`);
            resolve(null);
            return;
          }


          if (data.message === "done") {
            addLog("✔️ Simulation completed!");
            stopProcessing();
            es.close();
            resolve(true);
            return;
          }

          // 👉 извличаме името на турнира от първия лог
          if (!firstMessageHandled && data.message.startsWith("🏆")) {
            const tournamentName = data.message.split(":")[0].replace("🏆", "").trim().split("—")[0].trim();
            flushSync(() => {
              setLogs([`Simulating ${tournamentName} matches...`]);
            });
            firstMessageHandled = true;
          }

          addLog(data.message);
        } catch {
          addLog("⚠️ Invalid data received.");
        }
      };

      es.onerror = () => {
        addLog("❌ Connection error");
        stopProcessing();
        es.close();
        resolve(null);
      };
    });
  };

  const runNextDay = async (gameSaveId) => {
  startProcessing("⏩ Advancing to next day...", { allowCancel: false });

  const es = new EventSource(`/api/gameday/current/next-day-stream/${gameSaveId}`);
  eventSourceRef.current = es;

  return new Promise((resolve) => {
    es.onmessage = (e) => {
      if (!e.data) return;
      try {
        const data = JSON.parse(e.data);

        if (data.error) {
          addLog(`❌ ${data.error}`);
          stopProcessing();
          es.close();
          resolve(null);
          return;
        }

        if (data.message === "done") {
          addLog("✔️ Day advanced successfully!");
          stopProcessing();
          es.close();
          resolve(true);
          return;
        }

        addLog(data.message);
      } catch {
        addLog("⚠️ Invalid data received.");
      }
    };

    es.onerror = () => {
      addLog("❌ Connection error");
      stopProcessing();
      es.close();
      resolve(null);
    };
  });
};


  return (
    <ProcessingContext.Provider
      value={{
        startProcessing,
        addLog,
        stopProcessing,
        runSimulateMatches,
        runNextDay,
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
