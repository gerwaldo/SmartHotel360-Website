import { applyMiddleware, combineReducers, compose, createStore } from 'redux';
import { thunk } from 'redux-thunk';
import * as ConferenceRoomsFeatures from './ConferenceRoomsFeatures';
import * as Home from './Home';
import * as NavMenu from './NavMenu';
import * as Pets from './Pets';
import * as RoomDetail from './RoomDetail';
import * as Rooms from './Rooms';
import * as Search from './Search';
import * as User from './User';

export default function configureStore(history, initialState) {
  const reducers = {
    conferenceRoomsFeatures: ConferenceRoomsFeatures.reducer,
    home: Home.reducer,
    nav: NavMenu.reducer,
    pets: Pets.reducer,
    roomDetail: RoomDetail.reducer,
    rooms: Rooms.reducer,
    search: Search.reducer,
    user: User.reducer
  };

  const middleware = [
    thunk
  ];

  // In development, use the browser's Redux dev tools extension if installed
  const enhancers = [];
  const isDevelopment = process.env.NODE_ENV === 'development';
  if (isDevelopment && typeof window !== 'undefined' && window.__REDUX_DEVTOOLS_EXTENSION__) {
    enhancers.push(window.__REDUX_DEVTOOLS_EXTENSION__());
  }

  const rootReducer = combineReducers({
    ...reducers
  });

  return createStore(
    rootReducer,
    initialState,
    compose(applyMiddleware(...middleware), ...enhancers)
  );
}
