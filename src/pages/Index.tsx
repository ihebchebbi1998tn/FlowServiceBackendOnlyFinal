const Index = () => {
  return (
    <main className="relative min-h-screen flex items-center justify-center overflow-hidden bg-gradient-to-br from-background via-background to-secondary/30">
      {/* Animated background elements */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute top-20 left-20 w-72 h-72 bg-primary/10 rounded-full blur-3xl animate-float" 
             style={{ animationDelay: "0s" }} />
        <div className="absolute bottom-20 right-20 w-96 h-96 bg-accent/10 rounded-full blur-3xl animate-float" 
             style={{ animationDelay: "2s" }} />
      </div>

      {/* Main content */}
      <div className="relative z-10 text-center px-6">
        <div className="space-y-8">
          {/* Main heading */}
          <h1 
            className="text-8xl md:text-9xl font-bold tracking-tight animate-fade-in-up"
            style={{ 
              background: "linear-gradient(135deg, hsl(var(--primary)), hsl(var(--accent)))",
              WebkitBackgroundClip: "text",
              WebkitTextFillColor: "transparent",
              backgroundClip: "text"
            }}
          >
            Hello World
          </h1>

          {/* Subtitle */}
          <p 
            className="text-xl md:text-2xl text-muted-foreground max-w-2xl mx-auto animate-fade-in-up"
            style={{ animationDelay: "0.2s", opacity: 0 }}
          >
            Welcome to your beautiful new app. Simple, elegant, and ready for anything.
          </p>

          {/* Decorative element */}
          <div 
            className="flex justify-center gap-2 animate-fade-in-up"
            style={{ animationDelay: "0.4s", opacity: 0 }}
          >
            <div className="w-2 h-2 rounded-full bg-primary animate-pulse" />
            <div className="w-2 h-2 rounded-full bg-accent animate-pulse" style={{ animationDelay: "0.2s" }} />
            <div className="w-2 h-2 rounded-full bg-primary animate-pulse" style={{ animationDelay: "0.4s" }} />
          </div>
        </div>
      </div>
    </main>
  );
};

export default Index;
