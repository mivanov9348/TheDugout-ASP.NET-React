import React, { useState } from "react";

const API_BASE = "https://localhost:7117/api/auth";

const AuthForm = ({ onAuthSuccess }) => {
  const [isRegister, setIsRegister] = useState(false);
  const [form, setForm] = useState({ username: "", email: "", password: "" });

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const endpoint = isRegister ? "register" : "login";

    try {
      const res = await fetch(`${API_BASE}/${endpoint}`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(form),
        credentials: "include", // важно за бисквитки
      });

      if (res.ok) {
        const data = await res.json();
        console.log("✅ Auth success:", data);
        onAuthSuccess();
      } else {
        const errorText = await res.text();
        console.error("❌ Auth error:", res.status, errorText);
        alert(`Грешка при вход/регистрация: ${res.status}`);
      }
    } catch (err) {
      console.error("❌ Fetch error:", err);
      alert("Няма връзка с бекенда. Провери дали е пуснат на https://localhost:7117");
    }
  };

  return (
    <div className="flex flex-col items-center justify-center h-screen bg-gray-900 text-white">
      <h2 className="text-3xl mb-4">{isRegister ? "Регистрация" : "Вход"}</h2>
      <form onSubmit={handleSubmit} className="flex flex-col space-y-3 w-64">
        {isRegister && (
          <input
            type="text"
            name="username"
            placeholder="Username"
            value={form.username}
            onChange={handleChange}
            className="p-2 rounded text-black"
          />
        )}
        <input
          type="email"
          name="email"
          placeholder="Email"
          value={form.email}
          onChange={handleChange}
          className="p-2 rounded text-black"
        />
        <input
          type="password"
          name="password"
          placeholder="Password"
          value={form.password}
          onChange={handleChange}
          className="p-2 rounded text-black"
        />
        <button className="bg-green-600 py-2 rounded hover:bg-green-500 transition">
          {isRegister ? "Регистрирай се" : "Влез"}
        </button>
      </form>
      <button
        onClick={() => setIsRegister(!isRegister)}
        className="mt-4 underline"
      >
        {isRegister ? "Имаш акаунт? Влез" : "Нямаш акаунт? Регистрирай се"}
      </button>
    </div>
  );
};

export default AuthForm;
