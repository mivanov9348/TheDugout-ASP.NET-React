/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {},
  },
  // ТУК Е ПРОМЯНАТА
  plugins: [
    require('tailwind-scrollbar'),
  ],
}