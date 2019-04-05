K_Mantel=2408;
T1_Mantel=12160;
Tu=1;
G_Mantel=tf([K_Mantel],[T1_Mantel 1])

K_Zelle =2321/K_Mantel;
T1_Zelle=807;
T2_Zelle=5;
Tu_Zelle=0.28*T1_Zelle;
Tg_Zelle=9.65*Tu_Zelle;
G_Zelle=tf([K_Zelle],[T2_Zelle*T1_Zelle T2_Zelle+T1_Zelle 1])

%Diskretisieren
G_Z_Mantel=c2d(G_Mantel,0.5)
G_Z_Zelle=c2d(G_Zelle,0.5);
%%
%Ziegler/Nichols
%PI Regler
Kp_M=(0.9/K_Mantel)*(T1_Mantel/Tu);
Tn_M=3.3*Tu;
Ki_M=Kp_M/Tn_M;
%{
Kp_M=1/K_Mantel;
Ki_M=Kp_M*T1_Mantel;
%}
Kp_Z=(0.9/K_Zelle)*(Tg_Zelle/Tu_Zelle);
Tn_Z=3.3*Tu_Zelle;
Ki_Z=Kp_Z/Tn_Z;
Kd_Z=0;

%%
%Ziegler/Nichols
%PID Regler
Kp_M=(0.9/K_Mantel)*(T1_Mantel/Tu);
Tn_M=3.3*Tu;
Ki_M=Kp_M/Tn_M;
%{
Kp_M=1/K_Mantel;
Ki_M=Kp_M*T1_Mantel;
%}
Kp_Z=(1.2*Tg_Zelle)/(K_Zelle*Tu_Zelle);
Tn_Z=2*Tu_Zelle;
Ki_Z=Kp_Z/Tn_Z;
Tv_Z=0.5*Tu_Zelle;
Kd_Z=Kp_Z*Tv_Z;
Tt_Z=sqrt(Tn_Z*Tv_Z);

%%
%Chien/Hrones und Reswick
%PI Regler
Kp_M=(0.35/K_Mantel)*(T1_Mantel/Tu);
Tn_M=1.2*Tu;
Ki_M=Kp_M/Tn_M;

Kp_Z=(0.35*Tg_Zelle)/(K_Zelle*Tu_Zelle);
Tn_Z=1.2*Tg_Zelle;
Ki_Z=Kp_Z/Tn_Z;
Kd_Z=0;
%%
%PID Chien/Hrones und Reswick
Kp_M=(0.35/K_Mantel)*(T1_Mantel/Tu);
Tn_M=1.2*Tu;
Ki_M=Kp_M/Tn_M;

Kp_Z=(0.6*Tg_Zelle)/(K_Zelle*Tu_Zelle);
Tn_Z=1*Tg_Zelle;
Ki_Z=Kp_Z/Tn_Z;
Tv_Z=0.5*Tu_Zelle;
Kd_Z=Kp_Z*Tv_Z;
Tt_Z=sqrt(Tn_Z*Tv_Z)
%%
PI=Kp_M*(tf([Tn_M 1],[Tn_M 0]));
x=PI*G_Mantel
w = logspace(0.00001,10,20);
bode(x)
%%
%PIDtuner
Kp_M=(0.35/K_Mantel)*(T1_Mantel/Tu);
Tn_M=1.2*Tu;
Ki_M=Kp_M/Tn_M;

Zelle=pidtune(G_Zelle,'PID',1.0)
Kp_Z=Zelle.Kp;
Ki_Z=Zelle.Ki;
Kd_Z=Zelle.Kd;

%%
% Simulation
close all
sim('Regler_Kaskadierung');

figure('Name','Temperaturverlauf Mantel');
subplot(2,2,1);
plot(P_M.time, P_M.signals.values,'b', I_M.time, I_M.signals.values,'r',W_M.time, W_M.signals.values, 'k')
title('Stellgrössen Mantel')
legend('P','I','Location','southeast') ;legend('boxoff')
subplot(2,2,2);
plot(T_M.time,T_M.signals.values)
%plot(, T_M.signals.values,'b', W_M.time, W_M.signals.values, 'g')
%plot(T_M.time, T_M.signals.values,'b',I_M.time, I_M.signals.values,'r', P_M.time, P_M.signals.values, 'g' )
title('T_{Mantel}')
legend('T_{Mantel}','Location','southeast') ;legend('boxoff')
%figure('Name','Temperaturverlauf Zelle');
subplot(2,2,3);
plot(P_Z.time, P_Z.signals.values,'b', I_Z.time, I_Z.signals.values, 'r', D_Z.time, D_Z.signals.values,'g', W_Z.time, W_Z.signals.values ,'k')
title('Stellgrössen Zelle')
legend('P','I','D','Location','southeast') ;legend('boxoff')
axis([0 24000 -150  150]) 
subplot(2,2,4);
plot(T_Z.time, T_Z.signals.values, E_Z.time, E_Z.signals.values)
%plot(T_Z.time, T_Z.signals.values,'b',I_Z.time, I_Z.signals.values,'r', P_Z.time, P_Z.signals.values, 'g' )
title('T_{Zelle}')
ylabel('Temperature [°C]'); xlabel('Time [s]'); 
legend('T_{Zelle}','Fehler','Location','southeast') ;legend('boxoff')
