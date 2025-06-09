import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactQueryDevtools } from '@tanstack/react-query-devtools';
import { BrowserRouter } from 'react-router-dom';

import { ThemeProvider } from "@/components/ui/theme-provider";

import AppRouter from './routes/AppRouter';

const queryClient = new QueryClient();

function App() {
    return (
        <ThemeProvider defaultTheme="light" >
            <BrowserRouter>
                <QueryClientProvider client={queryClient}>
                    <AppRouter />
                    <ReactQueryDevtools initialIsOpen={false} />
                </QueryClientProvider>
            </BrowserRouter>
        </ThemeProvider>
    );
}

export default App;
