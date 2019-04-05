K_Mantel=2408;
T1_Mantel=12160;
G_Mantel=tf([K_Mantel],[T1_Mantel 1]);

%Diskretisieren
G_Z_Mantel=c2d(G_Mantel,0.5);
%%
Kp_M=5;
%%
%Ziegler/Nichols
Kp_M=(1/K_Mantel)*(T1_Mantel/Tu);
Tn_M=3.3*Tu;
Ki_M=Kp_M/Tn_M;
%%
%Chien/Hrones und Reswick
Kp_M=(0.35/K_Mantel)*(T1_Mantel/Tu);
Tn_M=1.2*Tu;
Ki_M=Kp_M/Tn_M;

%%
% Simulation
close all
sim('Regler_Mantel');
figure('Name','Temperaturverlauf Regelung');
subplot(2,1,1);
plot(T_M.time, T_M.signals.values,'r')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Mantel}','Location','southeast') ;legend('boxoff')
subplot(2,1,2);
plot(e.time, e.signals.values,'r',P_M.time, P_M.signals.values, 'b')
axis([0 1000 0 1])
legend('Error','K','Location','northeast') ;legend('boxoff')
%%
plot(e.time, e.signals.values,'r')