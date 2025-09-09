import './global';

import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.css';
import 'react-datepicker/dist/react-datepicker.css';
import './scss/site.css';

import React from 'react';
import { createRoot } from 'react-dom/client';
import { Provider } from 'react-redux';
import { BrowserRouter } from 'react-router-dom';
import { createBrowserHistory } from 'history';
import configureStore from './store/configureStore';
import App from './App';
import { loadSettings } from './Settings';
import registerServiceWorker from './registerServiceWorker';

// Create browser history to use in the Redux store
const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href');
const history = createBrowserHistory({ basename: baseUrl });

// Get the application-wide store instance, prepopulating with state from the server where available.
const initialState = window.initialReduxState;
const store = configureStore(history, initialState);

const init = async () => {
    await loadSettings();

    const rootElement = document.getElementById('root');
    const root = createRoot(rootElement);

    root.render(
        <Provider store={store}>
            <BrowserRouter basename={baseUrl}>
                <App />
            </BrowserRouter>
        </Provider>
    );

    registerServiceWorker();
};

init();
