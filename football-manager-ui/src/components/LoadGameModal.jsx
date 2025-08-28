import React from "react";
import Swal from "sweetalert2";

function LoadGameModal({ saves, onClose, onSelectSave, onDeleteSave }) {
  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-60 z-50">
      <div className="bg-white rounded-2xl shadow-lg p-6 w-96">
        <h2 className="text-xl font-bold mb-4 text-center">Load Game</h2>

        {saves.length === 0 ? (
          <p className="text-gray-600 text-center">Нямаш сейфове.</p>
        ) : (
          <ul className="space-y-3">
            {saves.map((save) => (
              <li
                key={save.id}
                className="flex justify-between items-center border p-3 rounded-lg"
              >
                <div>
                  <p className="font-medium">{save.name}</p>
                  <p className="text-sm text-gray-500">
                    {new Date(save.createdAt).toLocaleString()}
                  </p>
                </div>
                <div className="flex gap-2">
                  <button
                    className="px-3 py-1 bg-blue-600 text-white rounded-lg hover:bg-blue-500 transition"
                    onClick={() => onSelectSave(save)}
                  >
                    Load
                  </button>
                  <button
                    className="px-3 py-1 bg-red-600 text-white rounded-lg hover:bg-red-500 transition"
                    onClick={() =>
                      Swal.fire({
                        title: "Изтриване?",
                        text: `Сигурен ли си, че искаш да изтриеш "${save.name}"?`,
                        icon: "warning",
                        showCancelButton: true,
                        confirmButtonText: "Да",
                        cancelButtonText: "Не",
                      }).then((result) => {
                        if (result.isConfirmed) {
                          onDeleteSave(save.id);
                        }
                      })
                    }
                  >
                    Delete
                  </button>
                </div>
              </li>
            ))}
          </ul>
        )}

        <div className="flex justify-center mt-6">
          <button
            onClick={onClose}
            className="px-6 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-400 transition"
          >
            Затвори
          </button>
        </div>
      </div>
    </div>
  );
}


export default LoadGameModal;
