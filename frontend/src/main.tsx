import React from 'react';
import ReactDOM from 'react-dom/client';
import { AppProviders } from '@/app/providers';
import { router } from '@/routes/router';
import '@/styles/index.css';
import '@fontsource-variable/manrope';
import '@fontsource/space-grotesk';

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <AppProviders router={router} />
  </React.StrictMode>,
);
