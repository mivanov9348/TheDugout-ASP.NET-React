import React, { useState, useEffect } from "react";
import Swal from "sweetalert2";

export default function CustomizeGame({ onStart, onClose }) {
    const [countries, setCountries] = useState([]);
    const [selected, setSelected] = useState([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const loadCountries = async () => {
            try {
                const res = await fetch("/api/games/countries", { credentials: "include" });
                if (!res.ok) throw new Error("Failed to load countries");
                const data = await res.json();
                setCountries(data);
                setSelected(data.map((c) => c.id)); // по подразбиране всички селектирани
            } catch (err) {
                console.error(err);
                Swal.fire("Error", "Failed to load countries list", "error");
            } finally {
                setLoading(false);
            }
        };

        loadCountries();
    }, []);

    const toggleCountry = (id) => {
        setSelected((prev) =>
            prev.includes(id) ? prev.filter((c) => c !== id) : [...prev, id]
        );
    };

    const handleStart = async () => {
        if (selected.length === 0) {
            Swal.fire("Wait!", "You must select at least one country!", "warning");
            return;
        }

        try {
            Swal.fire({
                title: "Applying your setup...",
                didOpen: () => Swal.showLoading(),
                allowOutsideClick: false,
            });

            const res = await fetch("/api/games/activate", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify(selected),
            });

            if (!res.ok) throw new Error("Failed to activate leagues and cups");
            Swal.close();
            onStart({ selectedCountries: selected });
        } catch (err) {
            console.error(err);
            Swal.fire("Error", "Could not apply customization.", "error");
        }
    };

    return (
        <div className="fixed inset-0 flex items-center justify-center bg-black bg-opacity-70 z-50 backdrop-blur-sm">
            <div className="bg-gray-900 text-white rounded-3xl shadow-2xl p-8 w-[650px] max-h-[85vh] overflow-y-auto border border-gray-700">
                <h2 className="text-3xl font-extrabold mb-4 text-center text-green-400">
                    🌍 Choose Your Football World
                </h2>
                <p className="text-sm text-gray-400 mb-6 text-center">
                    Select which countries’ leagues and cups to include in your game.
                </p>

                {loading ? (
                    <div className="text-center text-gray-400 py-10">Loading...</div>
                ) : (
                    <div className="grid grid-cols-2 gap-3 mb-8">
                        {countries.map((country) => (
                            <div
                                key={country.id}
                                onClick={() => toggleCountry(country.id)}
                                className={`flex items-center gap-3 p-3 rounded-xl cursor-pointer border transition-all duration-200 ${
                                    selected.includes(country.id)
                                        ? "bg-green-700 border-green-500 shadow-md scale-[1.02]"
                                        : "bg-gray-800 border-gray-700 hover:bg-gray-700"
                                }`}
                            >
                                <img
                                    src={`https://flagcdn.com/w40/${country.code?.toLowerCase()}.png`}
                                    alt={`${country.name} flag`}
                                    className="rounded-md w-8 h-6 border border-gray-600"
                                    onError={(e) => {
                                        e.target.style.display = "none";
                                    }}
                                />
                                <span className="flex-1 font-medium">{country.name}</span>
                                <input
                                    type="checkbox"
                                    checked={selected.includes(country.id)}
                                    readOnly
                                    className="w-5 h-5 accent-green-500 cursor-pointer"
                                />
                            </div>
                        ))}
                    </div>
                )}

                <div className="flex justify-between mt-4">
                    <button
                        onClick={onClose}
                        className="px-5 py-2 bg-gray-700 rounded-xl hover:bg-gray-600 transition shadow-md"
                    >
                        Cancel
                    </button>
                    <button
                        onClick={handleStart}
                        className="px-5 py-2 bg-green-600 rounded-xl hover:bg-green-500 transition shadow-lg"
                    >
                        Start Game
                    </button>
                </div>
            </div>
        </div>
    );
}
