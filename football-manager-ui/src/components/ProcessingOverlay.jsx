import React, { useEffect, useRef } from "react";

export default function ProcessingOverlay({
  logs,
  isProcessing,
  stopProcessing,
  allowCancel,
}) {
  const logEndRef = useRef(null);

  useEffect(() => {
    if (logEndRef.current) {
      logEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [logs]);

  if (!isProcessing) return null;

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 backdrop-blur-sm">
      <div className="bg-white w-full max-w-2xl rounded-2xl shadow-2xl overflow-hidden flex flex-col">
        <div className="bg-sky-700 text-white px-5 py-3 font-bold text-lg">
          Processing...
        </div>

        <div className="flex-1 overflow-y-auto p-4 bg-gray-50 font-mono text-sm text-gray-800 space-y-1 max-h-[400px]">
          {logs.map((line, idx) => (
            <div key={idx}>{line}</div>
          ))}
          <div ref={logEndRef} />
        </div>

        {allowCancel && (
          <div className="p-3 flex justify-end bg-gray-100 border-t">
            <button
              onClick={stopProcessing}
              className="px-4 py-2 rounded-lg bg-red-500 text-white font-semibold hover:bg-red-600 transition"
            >
              Cancel
            </button>
          </div>
        )}
      </div>
    </div>
  );
}