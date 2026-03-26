import type { Config } from 'tailwindcss';

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        ink: '#0f172a',
        steel: '#334155',
        mist: '#e2e8f0',
        paper: '#f8fafc',
        cloud: '#f1f5f9',
        accent: '#0f766e',
        accentSoft: '#ccfbf1',
        danger: '#b91c1c',
        warning: '#b45309',
        success: '#166534',
      },
      fontFamily: {
        sans: ['Manrope Variable', 'sans-serif'],
        display: ['Space Grotesk', 'sans-serif'],
      },
      boxShadow: {
        soft: '0 22px 50px -24px rgba(15, 23, 42, 0.28)',
      },
      backgroundImage: {
        'hero-glow': 'radial-gradient(circle at top left, rgba(15, 118, 110, 0.22), transparent 38%), radial-gradient(circle at top right, rgba(37, 99, 235, 0.14), transparent 30%), linear-gradient(135deg, rgba(255,255,255,0.92), rgba(248,250,252,0.95))',
      },
      keyframes: {
        rise: {
          '0%': { opacity: '0', transform: 'translateY(12px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        rise: 'rise 0.55s ease-out',
      },
    },
  },
  plugins: [],
} satisfies Config;
